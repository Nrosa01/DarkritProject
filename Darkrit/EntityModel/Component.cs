
using Darkrit.Base;
using Microsoft.Xna.Framework;

namespace Darkrit.EntityModel;

public interface IComponent
{
    public EntityRegistry World { get; init; }

    public Handle<Entity> EntityHandle { get; init; }

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

    public void Update(GameTime gameTime);
    public void FixedUpdate(GameTime gameTime);
}

public struct Component
{

}
