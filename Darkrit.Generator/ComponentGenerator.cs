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
                public partial struct {{typeName}} : IComponent
                {
                    public ref Entity Entity => ref World.GetEntity(EntityHandle); 

                    public EntityRegistry World { get; set; }
                    public Handle<Entity> EntityHandle { get; set; }
                    public bool Enabled { get; set; }

                    {{GenerateMethods(component, iComponent)}}
                }
                """;

            spc.AddSource($"{typeName}.g.cs", source);
        });
    }

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

    private static string GenerateMethod(INamedTypeSymbol type, string methodName)
    {
        if (HasMethod(type, methodName)) return string.Empty;

        return $"public void {methodName}(GameTime gameTime) {{ }}";
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

            var generated = GenerateMethod(type, method.Name);

            if (!string.IsNullOrEmpty(generated))
                builder.AppendLine(generated);
        }

        return builder.ToString().TrimEnd().Replace("\n", "\n    ");
    }
}