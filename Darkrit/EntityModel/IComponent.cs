
using System;
using System.Runtime.CompilerServices;
using Darkrit.Base;
using Microsoft.Xna.Framework;

namespace Darkrit.EntityModel;

/// <summary>
/// Attribute that's used in source generation to create getters
/// to the handle and reference of the component of type <paramref name="componentType"/>
/// If the entity doesn't have said component, a valid empty value is returned
/// </summary>
/// <param name="componentType">Component type to inject</param>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
public sealed class InjectComponentAttribute(Type componentType) : Attribute
{
    /// <summary>
    /// The type of the component to inject
    /// </summary>
    public Type ComponentType { get; } = componentType;
}

/// <summary>
/// Allows to define a priority for the component type
/// The lower, the more priority it has
/// </summary>
/// <param name="priorityi"></param>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class PriorityAttribute(int priorityi) : Attribute {
    /// <summary>
    /// The priority of the component
    /// </summary>
    public int Priority { get; } = priorityi;
}

/// <summary>
/// Attribute to mark that an struct is a Component
/// It implements <see cref="IComponent"/> under the hood, as well
/// as the default body for not implemented functions
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class ComponentAttribute : Attribute { }

/// <summary>
/// Defines whether this component executes the <see cref="IComponent.FixedUpdate(GameTime)"/> callback
/// This is generated automatically when you implement the method, don't implement it on your own
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class FixedUpdateableAttribute : Attribute { }


/// <summary>
/// Defines whether this component executes the <see cref="IComponent.Update(GameTime)"/> callback
/// This is generated automatically when you implement the method, don't implement it on your own
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class UpdateableAttribute : Attribute { }

/// <summary>
/// Defines whether this component executes the <see cref="IComponent.LateUpdate(GameTime)"/> callback
/// This is generated automatically when you implement the method, don't implement it on your own
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class LateUpdateableAttribute : Attribute { }

/// <summary>
/// Defines whether this component executes the <see cref="IComponent.Draw(GameTime)"/> callback
/// This is generated automatically when you implement the method, don't implement it on your own
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class DrawableAttribute : Attribute { }


/// <summary>
/// This is used for the source generator, so it can generate the previous attributes
/// from its corresponding functions. Just to make my life easier. Check the component
/// generator code to get more information
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class AutoRegisterAttribute : Attribute { }

/// <summary>
/// Marks a private field or property backing field as editable in the editor.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class SerializeFieldAttribute : Attribute
{
}

/// <summary>
/// Marks a private field or property backing field as editable in the editor.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class ShowInInspectorAttribute : Attribute
{
}

/// <summary>
/// Used for vectors that need to be linked together
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class LinkableAttribute : Attribute
{
}

/// <summary>
/// Defines all the required properties a component needs
/// </summary>
public interface IComponent
{
    /// <summary>
    /// A reference to the World that owns this component
    /// </summary>
    public EntityRegistry World { get; set; }

    /// <summary>
    /// Handle to the entity that owns this compoent
    /// </summary>
    public Handle<Entity> EntityHandle { get; set; }

    /// <summary>
    /// Whether this Component is enabled
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Whether this component's <see cref="EntityHandle"/> is active in the hierarchy
    /// </summary>
    public bool ActiveSelf 
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => World.GetEntity(EntityHandle).ActiveSelf; 
    }

    /// <summary>
    /// Whether this component's <see cref="EntityHandle"/> is active in the scene
    /// Say, Entity A has a child Entity B. B could be active but A 
    /// be inactive, which would result in B <see cref="ActiveInHierachy"/> be false
    /// while <see cref="ActiveSelf"/> is true
    /// </summary>
    public bool ActiveInHierachy 
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => World.GetEntity(EntityHandle).ActiveInHierarchy; 
    }

    /// <summary>
    /// Called when the component is added to the entity
    /// </summary>
    void OnAdd();

    /// <summary>
    /// Called on each Update cycle
    /// </summary>
    /// <param name="gameTime"></param>
    [AutoRegister]
    void Update(GameTime gameTime);
    
    /// <summary>
    /// Called before Render, after <see cref="Update(GameTime)"/> and <see cref="FixedUpdate(GameTime)"/> function
    /// </summary>
    /// <param name="gameTime"></param>
    [AutoRegister]
    void LateUpdate(GameTime gameTime);

    /// <summary>
    /// Callback that is called <see cref="Core.PHYSICS_TICKS_PER_SECOND"/> per second
    /// It might be called more than one per frame, or maybe even not be called at all
    /// </summary>
    /// <param name="gameTime"></param>
    [AutoRegister]
    void FixedUpdate(GameTime gameTime);
 
    /// <summary>
    /// Callback where all drawing operations must be done
    /// </summary>
    /// <param name="gameTime"></param>
    [AutoRegister]
    void Draw(GameTime gameTime);

    /// <summary>
    /// Called when the component <see cref="Enabled"/> is set to true
    /// Or when any of its parent entities are enabled while this component is enabled
    /// </summary>
    void OnEnable();

    /// <summary>
    /// Called when the component <see cref="Enabled"/> is set to false
    /// Or when any of its parent entities are disabled while this component is enabled
    /// </summary>
    void OnDisable();
    
    /// <summary>
    /// Called just before being removed from an entity
    /// </summary>
    void OnRemove();
}