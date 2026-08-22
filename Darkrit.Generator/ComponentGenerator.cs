using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Darkrit.Generator;

[Generator]
public sealed class ComponentGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var components = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Darkrit.EntityModel.ComponentAttribute",
                static (node, _) => node is StructDeclarationSyntax,
                static (context, _) => (INamedTypeSymbol)context.TargetSymbol);

        var componentInterface = context.CompilationProvider.Select(static (compilation, _) =>
                                 compilation.GetTypeByMetadataName("Darkrit.EntityModel.IComponent"));

        var input = components.Combine(componentInterface);

        context.RegisterSourceOutput(input, static (spc, data) =>
        {
            var component = data.Left;
            var iComponent = data.Right;

            var namespaceName = component.ContainingNamespace.ToDisplayString();
            var typeName = component.Name;

            var source = $$"""
                using Microsoft.Xna.Framework;
                using System.Runtime.InteropServices;
                using global::Darkrit.EntityModel;
                using global::Darkrit.Base;

                namespace {{namespaceName}};

                {{GenerateAttributes(component, iComponent)}}
                [StructLayout(LayoutKind.Auto)]
                public partial struct {{typeName}} : IComponent, IHandle<{{typeName}}>
                {
                    /// <inheritdoc/>
                    public Handle<{{typeName}}> Handle { get; set; }

                    /// <summary>
                    /// A reference to the entity that owns this component
                    /// </summary>
                    public readonly ref Entity Entity => ref World.GetEntity(EntityHandle); 

                    /// <inheritdoc/>
                    public EntityRegistry World { get; set; }
                    
                    /// <inheritdoc/>
                    public Handle<Entity> EntityHandle { get; set; }
                    
                    /// <inheritdoc/>
                    public bool Enabled
                    {
                        get; 
                        set
                        {
                            if (field == value)
                                return;

                            field = value;

                            if (!Entity.ActiveInHierarchy) return;

                            if (field)
                                OnEnable();
                            else
                                OnDisable();
                        }
                    } = true;
                    {{GenerateConstructor(component)}}{{GenerateInjectedComponents(component)}}

                    {{GenerateMethods(component, iComponent)}}
                }
                """;

            spc.AddSource($"{typeName}.g.cs", source);
        });
    }

    private static IEnumerable<INamedTypeSymbol> GetInjectedComponents(INamedTypeSymbol type)
    {
        foreach (var attribute in type.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != "Darkrit.EntityModel.InjectComponentAttribute")
                continue;

            if (attribute.ConstructorArguments.Length == 0)
                continue;

            if (attribute.ConstructorArguments[0].Value is INamedTypeSymbol component)
                yield return component;
        }
    }

    private static string GenerateInjectedComponents(INamedTypeSymbol type)
    {
        StringBuilder builder = new();

        foreach (var component in GetInjectedComponents(type))
        {
            var componentName = component.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            var propertyName = component.Name;

            builder.AppendLine($$"""
            /// <summary>
            /// Direct reference to the specific component store of type <see cref="{{componentName}}"/>
            /// </summary>
            private ComponentStore<{{componentName}}> {{propertyName}}Store => field ??= World.GetStore<{{componentName}}>();

            /// <summary>
            /// Property to the component of type of type <see cref="{{componentName}}"/>
            /// If the component doesn't exist yet in the entity it returns the empty handle
            /// It's advised to check if the Entity has this handle
            /// </summary>
            private Handle<{{componentName}}> {{propertyName}}Handle
            {
                get
                {
                    if (field.Id == 0)
                        field = Entity.GetComponentHandle<{{componentName}}>();

                    return field;
                }
            }

            /// <summary>
            /// Direct reference to the specific component of type <see cref="{{componentName}}"/>
            /// If the entity doesn't have the component, a default one is returned
            /// </summary>
            public ref {{componentName}} {{propertyName}} => ref {{propertyName}}Store.Get({{propertyName}}Handle);
            """);
        }

        return builder.ToString().TrimEnd().Replace("\n", "\n    ");
    }

    private static string GenerateConstructor(INamedTypeSymbol type)
    {
        if (HasConstructor(type))
            return string.Empty;

        return $"public {type.Name}() {{ }}";
    }

    private static bool HasConstructor(INamedTypeSymbol type) => type.InstanceConstructors.Any(c => !c.IsImplicitlyDeclared);
    private static bool HasMethod(INamedTypeSymbol type, string name) => type.GetMembers(name).OfType<IMethodSymbol>().Any(m => !m.IsImplicitlyDeclared);
    private static string GenerateAttributes(INamedTypeSymbol type, INamedTypeSymbol? interfaceType)
    {
        if (interfaceType is null)
            return "";

        StringBuilder builder = new();

        foreach (var method in interfaceType.GetMembers().OfType<IMethodSymbol>())
        {
            var callable = method.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Darkrit.EntityModel.AutoRegisterAttribute");

            if (!callable)
                continue;

            if (HasMethod(type, method.Name))
                builder.AppendLine($"[{method.Name}able]");
        }

        return builder.ToString().TrimEnd();
    }

    private static string GenerateMethod(INamedTypeSymbol type, IMethodSymbol method)
    {
        if (HasMethod(type, method.Name)) return string.Empty;

        var parameters = string.Join(", ", method.Parameters.Select(p =>
            $"{p.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} {p.Name}"
        ));

        return $$"""
            /// <inheritdoc/>
            public void {{method.Name}}({{parameters}}) { }
            """;
    }

    private static string GenerateMethods(INamedTypeSymbol type, INamedTypeSymbol? interfaceType)
    {
        // This shouldn't happen...
        if (interfaceType is null) return "";

        StringBuilder builder = new();

        foreach (var method in interfaceType.GetMembers().OfType<IMethodSymbol>())
        {
            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            var generated = GenerateMethod(type, method);

            if (!string.IsNullOrEmpty(generated))
                builder.AppendLine(generated);
        }

        return builder.ToString().TrimEnd().Replace("\n", "\n    ");
    }
}