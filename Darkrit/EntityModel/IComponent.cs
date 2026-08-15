
using System;
using Darkrit.Base;
using Microsoft.Xna.Framework;

namespace Darkrit.EntityModel;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class UpdateableAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Struct)]
public sealed class RenderableAttribute : Attribute { }

public interface IComponent
{
    public EntityRegistry World { get; set; }

    public Handle<Entity> EntityHandle { get; set; }

    public ref Entity Entity => ref World.GetEntity(EntityHandle);

    /// <summary>
    /// Whether this Component is enabled
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Whether this component's <see cref="EntityHandle"/> is active in the hierarchy
    /// </summary>
    public bool ActiveSelf { get => World.GetEntity(EntityHandle).ActiveSelf; }

    /// <summary>
    /// Whether this component's <see cref="EntityHandle"/> is active in the scene
    /// Say, Entity A has a child Entity B. B could be active but A 
    /// be inactive, which would result in B <see cref="ActiveInHierachy"/> be false
    /// while <see cref="ActiveSelf"/> is true
    /// </summary>
    public bool ActiveInHierachy { get => World.GetEntity(EntityHandle).ActiveInHierachy; }

    void Start() { }

    void Update(GameTime gameTime) { }
    void FixedUpdate(GameTime gameTime) { }
    void Draw(GameTime gameTime) { }
}