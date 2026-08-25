using Darkrit.Base;
using Darkrit.Physics.Boxy2D;
using Microsoft.Xna.Framework;
using Darkrit.Utilities;
using System.Runtime.CompilerServices;

namespace Darkrit.EntityModel.Components;

[Component]
public partial struct PhysicsBody
{
    Handle<Body<Handle<Entity>>> _physicsHandle;

    [ShowInInspector] bool _showCollider = true;

    readonly ref Body<Handle<Entity>> Body => ref World.Physics.Get(_physicsHandle);

    Vector2 baseSize = Vector2.One * 24;
    Vector2 previousScale;

    public Vector2 Velocity;

    public Vector2 Size
    {
        readonly get => baseSize;
        set
        {
            baseSize = value;
            Body.Bounds = Body.Bounds with { Size = value * Entity.Scale };
        }
    }

    public void OnCreate()
    {
        _physicsHandle = World.Physics.Create(Entity.Position, baseSize, 1, 1, EntityHandle);
    }

    public void Start()
    {
        previousScale = Entity.Scale;
    }

    public void OnEnable()
    {
        if(_physicsHandle.Id  == 0)
            _physicsHandle = World.Physics.Create(Entity.Position, baseSize, 1, 1, EntityHandle);
    }

    public void OnDisable()
    {
        if(_physicsHandle.Id  != 0)
            World.Physics.Remove(_physicsHandle);
    }

    public void MoveAndSlide()
    {
        World.Physics.Move(_physicsHandle, ref Velocity);
        Entity.Position = Body.Bounds.Location;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LateUpdate(GameTime gameTime)
    {
        if (Entity.Scale != previousScale)
        {
            Size = baseSize;
            previousScale = Entity.Scale;
        }

        World.Physics.Teleport(_physicsHandle, Entity.Position);
    }

    public void Draw(GameTime gameTime)
    {
        if(_showCollider)
        {
            // Set here to Entity.Position to benefit from physics interpolation
            var bounds = Body.Bounds with { Location = Entity.Position };
            Core.SpriteBatch.Draw(bounds, Color.Red, 0.5f);
        }
    }
}
