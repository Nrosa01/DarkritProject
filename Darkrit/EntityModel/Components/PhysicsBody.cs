using Darkrit.Base;
using Darkrit.DevTools.Logger;
using Darkrit.Physics.Boxy2D;
using Darkrit.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;

namespace Darkrit.EntityModel.Components;

[Component]
public partial struct PhysicsBody
{
    Handle<Body<Handle<Entity>>> _physicsHandle;

    [ShowInInspector] bool _showCollider = true;

    Vector2 baseSize = Vector2.One * 24;
    Vector2 previousScale;

    public Vector2 Velocity;

    public readonly ref Body<Handle<Entity>> Body => ref World.Physics.Get(_physicsHandle);

    public readonly ReadOnlySpan<CollisionHit<Body<Handle<Entity>>>> Collisions => World.Physics.LastCollsions;

    [ShowInInspector, ReadOnly] bool _isOnFloor;
    [ShowInInspector, ReadOnly] bool _isOnWall;
    [ShowInInspector, ReadOnly] bool _isOnCeiling;

    public readonly bool IsOnFloor => _isOnFloor;
    public readonly bool IsOnWall => _isOnWall;
    public readonly bool IsOnCeiling => _isOnCeiling;

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
        _physicsHandle = World.Physics.Create(
            Entity.Position,
            baseSize,
            1,
            1,
            EntityHandle
        );
    }

    public void Start()
    {
        previousScale = Entity.Scale;
    }

    public void OnEnable()
    {
        if (_physicsHandle.Id == 0)
        {
            _physicsHandle = World.Physics.Create(
                Entity.Position,
                baseSize,
                1,
                1,
                EntityHandle
            );
        }
    }

    public void OnDisable()
    {
        if (_physicsHandle.Id != 0)
            World.Physics.Remove(_physicsHandle);
    }

    public void MoveAndSlide(GameTime gameTime)
    {
        if (Velocity == Vector2.Zero) return;

        Vector2 motion = Velocity * gameTime.Delta;

        _isOnFloor = false;
        _isOnWall = false;
        _isOnCeiling = false;

        World.Physics.Move(
            _physicsHandle,
            ref motion,
            CollisionFilters<Handle<Entity>>.Response(CollisionResponses.Slide)
        );

        Velocity = motion / gameTime.Delta;

        Entity.Position = Body.Bounds.Location;

        foreach (var collision in World.Physics.LastCollsions)
        {
            if (collision.Normal.Y < -0.5f)
                _isOnFloor = true;
            else if (collision.Normal.Y > 0.5f)
                _isOnCeiling = true;
            else if (MathF.Abs(collision.Normal.X) > 0.5f)
                _isOnWall = true;
        }
    }

    public void Teleport(Vector2 position)
    {
        World.Physics.Teleport(_physicsHandle, position);
        Entity.Position = position;
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
        if (_showCollider)
        {
            var bounds = Body.Bounds with { Location = Entity.Position };
            Core.SpriteBatch.Draw(bounds, Color.Red, 0.5f);
        }
    }
}