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
    public Vector2 upDirection = new(0, -1);

    Handle<Body<Handle<Entity>>> _physicsHandle;

    [ShowInInspector] bool _showCollider = true;

    Vector2 baseSize = Vector2.One * 24;
    Vector2 previousScale;

    [SerializeField] readonly float floorSnapLength = 2f;

    public Vector2 Velocity;

    public readonly ref Body<Handle<Entity>> Body => ref World.Physics.Get(_physicsHandle);

    public readonly ReadOnlySpan<CollisionHit<Body<Handle<Entity>>>> Collisions => World.Physics.LastCollsions;

    bool _wasOnFloor;
    [ShowInInspector, ReadOnly] bool _isOnFloor;

    bool _wasOnLeftWall;
    bool _wasOnRightWall;

    [ShowInInspector, ReadOnly] bool _isOnLeftWall;
    [ShowInInspector, ReadOnly] bool _isOnRightWall;

    [ShowInInspector, ReadOnly]  bool _isOnCeiling;

    public readonly bool IsOnFloor => _isOnFloor;
    public readonly bool IsOnLeftWall => _isOnLeftWall;
    public readonly bool IsOnRightWall => _isOnRightWall;

    public readonly bool IsOnWall => _isOnLeftWall || _isOnRightWall;

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
        _wasOnFloor = _isOnFloor;
        _wasOnLeftWall = _isOnLeftWall;
        _wasOnRightWall = _isOnRightWall;

        _isOnFloor = false;
        _isOnLeftWall = false;
        _isOnRightWall = false;
        _isOnCeiling = false;

        // Keep the body attached to the floor when moving horizontally.
        if (_wasOnFloor && Vector2.Dot(Velocity, upDirection) <= 0f && floorSnapLength > 0f)
        {
            Vector2 snapMotion = -upDirection * floorSnapLength;

            if (World.Physics.Move(
                _physicsHandle,
                ref snapMotion,
                CollisionFilters<Handle<Entity>>.Response(CollisionResponses.Stop),
                testOnly: true))
            {
                _isOnFloor = true;
            }
        }

        if (_wasOnLeftWall)
        {
            Vector2 wallMotion = -Vector2.UnitX * floorSnapLength;

            if (World.Physics.Move(
                _physicsHandle,
                ref wallMotion,
                CollisionFilters<Handle<Entity>>.Response(CollisionResponses.Stop),
                testOnly: true))
            {
                _isOnLeftWall = true;
            }
        }

        if (_wasOnRightWall)
        {
            Vector2 wallMotion = Vector2.UnitX * floorSnapLength;

            if (World.Physics.Move(
                _physicsHandle,
                ref wallMotion,
                CollisionFilters<Handle<Entity>>.Response(CollisionResponses.Stop),
                testOnly: true))
            {
                _isOnRightWall = true;
            }
        }

        Vector2 motion = Velocity * gameTime.Delta;

        World.Physics.Move(_physicsHandle, ref motion, CollisionFilters<Handle<Entity>>.Response(CollisionResponses.Slide));

        Velocity = motion / gameTime.Delta;
        Entity.Position = Body.Bounds.Location;

        foreach (var collision in World.Physics.LastCollsions)
        {
            if (Vector2.Dot(collision.Normal, upDirection) > 0.5f)
                _isOnFloor = true;
            else if (Vector2.Dot(collision.Normal, upDirection) < -0.5f)
                _isOnCeiling = true;
            else if (collision.Normal.X > 0.5f)
                _isOnLeftWall = true;
            else if (collision.Normal.X < -0.5f)
                _isOnRightWall = true;
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