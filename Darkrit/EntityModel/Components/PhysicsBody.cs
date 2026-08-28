using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Darkrit.Base;
using Darkrit.DevTools.Logger;
using Darkrit.Physics.Boxy2D;
using Darkrit.Utilities;
using Microsoft.Xna.Framework;

namespace Darkrit.EntityModel.Components;

public static class EntityCollisionFilters
{
    public static CollisionResponseFunction Platform(ref Body<Handle<PhysicsBody>> self, ref Body<Handle<PhysicsBody>> other, EntityRegistry registry)
    {
        if (registry.PhysicsBodyStore.Get(other.UserData).IsTrigger)
            return CollisionResponses.Cross;

        return CollisionResponses.Slide;
    }
}

[Component, Priority(int.MaxValue)]
public partial struct PhysicsBody
{
    public enum PlatformOnLeave
    {
        AddVelocity,
        AddUpwardVelocity,
        None
    }

    // Configuration

    [Header("Shape")]
    [SerializeField] Vector2 baseSize = Vector2.One * 24;
    [SerializeField] Vector2 offset;
    [SerializeField] public bool IsTrigger { get; private set; }

    [Header("Movement")]
    public Vector2 upDirection = new(0, -1);
    [SerializeField] readonly float floorSnapLength = 2f;

    [Header("Platforms")]
    [SerializeField] PlatformOnLeave platformOnLeave = PlatformOnLeave.AddVelocity;

    [Header("Debug")]
    [ShowInInspector] bool _showCollider = true;

    // Runtime state

    public Vector2 Velocity;

    [Header("Collisions")]
    [ShowInInspector, ReadOnly] bool _isOnFloor;
    [ShowInInspector, ReadOnly] bool _isOnLeftWall;
    [ShowInInspector, ReadOnly] bool _isOnRightWall;
    [ShowInInspector, ReadOnly] bool _isOnCeiling;

    // Internal state

    bool _wasOnFloor;
    bool _wasOnLeftWall;
    bool _wasOnRightWall;

    Handle<Body<Handle<PhysicsBody>>> _platformHandle;
    Vector2 _platformPreviousPosition;
    Vector2 previousScale;

    // Dependencies

    Handle<Body<Handle<PhysicsBody>>> _physicsHandle;

    readonly ComponentStore<PhysicsBody> PhysicsBodyStore => World.PhysicsBodyStore;

    // Public API

    public readonly ref Body<Handle<PhysicsBody>> Body => ref World.Physics.Get(_physicsHandle);

    public readonly ReadOnlySpan<CollisionHit<Body<Handle<PhysicsBody>>>> Collisions => World.Physics.LastCollsions;

    public readonly bool IsOnFloor => _isOnFloor;
    public readonly bool IsOnLeftWall => _isOnLeftWall;
    public readonly bool IsOnRightWall => _isOnRightWall;
    public readonly bool IsOnWall => _isOnLeftWall || _isOnRightWall;
    public readonly bool IsOnCeiling => _isOnCeiling;

    // These two fields are for serialization and to edit in the inspector
    // In runtime I use the properties below, given these variables are NEVER
    // read, I don't need ton sync them with the Body properties
    // That's why the properties don't also set these values
    [OnEditorChange(nameof(ApplyCollisionFilter))]
    [SerializeField] uint _layer = 1;
    [OnEditorChange(nameof(ApplyCollisionFilter))]
    [SerializeField] uint _mask = 1;

    readonly uint Layer
    {
        get => Body.Layer; set => Body.Layer = value;
    }

    readonly uint Mask
    {
        get => Body.Mask; set => Body.Mask = value;
    }

    void ApplyCollisionFilter()
    {
        Body.Layer = _layer;
        Body.Mask = _mask;
    }

    public Vector2 Offset
    {
        readonly get => offset;
        set
        {
            offset = value;
            SyncPosition();
        }
    }

    public Vector2 Size
    {
        readonly get => baseSize;
        set
        {
            baseSize = value;
            Body.Bounds = Body.Bounds with { Size = value * Entity.Scale };
            SyncPosition();
        }
    }

    // Collision filtering

    CollisionFilterFunction<Handle<PhysicsBody>, EntityRegistry> _collisionFilter = EntityCollisionFilters.Platform;

    public void OnCreate()
    {
        _physicsHandle = World.Physics.Create(
            Entity.Position,
            baseSize * Entity.Scale,
            1,
            1,
            Handle
        );

        SyncPosition();
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
                baseSize * Entity.Scale,
                1,
                1,
                Handle
            );

            SyncPosition();
        }
    }

    public void OnDisable()
    {
        if (_physicsHandle.Id != 0)
            World.Physics.Remove(_physicsHandle);
    }

    public void MoveAndSlide(GameTime gameTime)
    {
        var previousPlatform = _platformHandle;
        _platformHandle = default;

        _wasOnFloor = _isOnFloor;
        _wasOnLeftWall = _isOnLeftWall;
        _wasOnRightWall = _isOnRightWall;

        _isOnFloor = false;
        _isOnLeftWall = false;
        _isOnRightWall = false;
        _isOnCeiling = false;

        if (_wasOnFloor && Vector2.Dot(Velocity, upDirection) <= 0f && floorSnapLength > 0f)
        {
            Vector2 snapMotion = -upDirection * floorSnapLength;

            if (World.Physics.Move(
                _physicsHandle,
                ref snapMotion,
                CollisionFilters<Handle<PhysicsBody>, EntityRegistry>.Response(CollisionResponses.Stop),
                World,
                testOnly: true))
            {
                _isOnFloor = true;
                _platformHandle = previousPlatform;
            }
        }

        if (_wasOnLeftWall)
        {
            Vector2 wallMotion = -Vector2.UnitX * floorSnapLength;

            if (World.Physics.Move(
                _physicsHandle,
                ref wallMotion,
                CollisionFilters<Handle<PhysicsBody>, EntityRegistry>.Response(CollisionResponses.Stop),
                World,
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
                CollisionFilters<Handle<PhysicsBody>, EntityRegistry>.Response(CollisionResponses.Stop),
                World,
                testOnly: true))
            {
                _isOnRightWall = true;
            }
        }

        Vector2 motion = Velocity * gameTime.Delta;

        World.Physics.Move(_physicsHandle, ref motion, _collisionFilter, World);

        Velocity = motion / gameTime.Delta;

        Entity.Position = Body.Bounds.Location
                         + Body.Bounds.Size * 0.5f
                         - GetWorldOffset();

        foreach (CollisionHit<Body<Handle<PhysicsBody>>> collision in World.Physics.LastCollsions)
        {
            if (Vector2.Dot(collision.Normal, upDirection) > 0.5f)
            {
                _isOnFloor = true;
                _platformHandle = collision.Handle;
                _platformPreviousPosition = World.Physics.Get(collision.Handle).Bounds.Location;
            }
            else if (Vector2.Dot(collision.Normal, upDirection) < -0.5f)
                _isOnCeiling = true;
            else if (collision.Normal.X > 0.5f)
                _isOnLeftWall = true;
            else if (collision.Normal.X < -0.5f)
                _isOnRightWall = true;
        }

        if (previousPlatform.Id != 0 && _platformHandle.Id == 0)
        {
            Vector2 platformPosition = World.Physics.Get(previousPlatform).Bounds.Location;
            Vector2 platformVelocity = (platformPosition - _platformPreviousPosition) / gameTime.Delta;

            switch (platformOnLeave)
            {
                case PlatformOnLeave.AddVelocity:
                    Velocity += platformVelocity;
                    break;

                case PlatformOnLeave.AddUpwardVelocity:
                    float upwardVelocity = Vector2.Dot(platformVelocity, upDirection);

                    if (upwardVelocity > 0f)
                        Velocity += upDirection * upwardVelocity;

                    break;
            }
        }
    }

    public void FixedUpdate(GameTime gameTime)
    {
        if (_platformHandle.Id == 0)
            return;

        Vector2 platformPosition = World.Physics.Get(_platformHandle).Bounds.Location;
        Vector2 platformDelta = platformPosition - _platformPreviousPosition;

        if (platformDelta != Vector2.Zero)
        {
            Entity.Position += platformDelta;
            SyncPosition();
        }

        _platformPreviousPosition = platformPosition;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Teleport(Vector2 position)
    {
        Entity.Position = position;
        SyncPosition();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LateUpdate(GameTime gameTime)
    {
        if (Entity.Scale != previousScale)
        {
            Size = baseSize;
            previousScale = Entity.Scale;
        }

        SyncPosition();
    }

    public void Draw(GameTime gameTime)
    {
        if (_showCollider)
        {
            Vector2 center = Entity.Position + GetWorldOffset();

            var bounds = Body.Bounds with
            {
                Location = center - Body.Bounds.Size * 0.5f
            };

            Core.SpriteBatch.Draw(bounds, Color.Red, 0.5f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SyncPosition()
    {
        Vector2 center = Entity.Position + GetWorldOffset();

        World.Physics.Teleport(_physicsHandle, center - Body.Bounds.Size * 0.5f);
    }

    readonly Vector2 GetWorldOffset()
    {
        Vector2 scaledOffset = offset * Entity.Scale;

        return Vector2.Transform(scaledOffset, Matrix.CreateRotationZ(Entity.Rotation));
    }
}