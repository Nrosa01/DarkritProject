using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Darkrit.Base;
using Darkrit.DataStructures;
using Darkrit.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RectangleF = Darkrit.Math.RectangleF;

namespace Darkrit.Physics.Boxy2D;

/// <summary>
/// Class that handles physics <see cref="Body{T}"/> items
/// </summary>
/// <typeparam name="T">The type of the custom <see cref="Body{T}.UserData"/>. If none is wanted use an empty struct</typeparam>
public class World<T>
{
    private readonly HandleMapGrowing<Body<T>> _bodies = [];
    private readonly GrowableArray<CollisionHit<Body<T>>> _lastCollisions = [];

    public ReadOnlySpan<CollisionHit<Body<T>>> LastCollsions => _lastCollisions.AsReadOnlySpan();

    /// <summary>
    /// Creates a new physics object in the world with the bounds defined by <paramref name="center"/> and <paramref name="size"/>
    /// </summary>
    /// <param name="center">Center of the AABB</param>
    /// <param name="size">Size of the AABB</param>
    /// <param name="layer">Bitmask layer this AABB is in</param>
    /// <param name="mask">Bitmask layer this AABB checks for when moving</param>
    /// <param name="userData">Optional parameter defined by the type <typeparamref name="T"/></param>
    /// <returns>A handle to the body. Be aware that this is a non mutable copy</returns>
    public Handle<Body<T>> Create(Vector2 center, Vector2 size, uint layer = 0, uint mask = 0, T userData = default)
    {
        return Create(new RectangleF
        {
            X = center.X - size.X * 0.5f,
            Y = center.Y - size.Y * 0.5f,
            Size = size
        },
        layer,
        mask,
        userData
        );
    }

    /// <summary>
    /// Creates a new physics object in the world with the bounds defined by <paramref name="rectangle"/>
    /// </summary>
    /// <param name="rectangle">The AABB rectangle that represents the Bounds of the item</param>
    /// <param name="layer">Bitmask layer this AABB is in</param>
    /// <param name="mask">Bitmask layer this AABB checks for when moving</param>
    /// <param name="userData">Optional parameter defined by the type <typeparamref name="T"/></param>
    /// <returns>A handle to the body. Be aware that this is a non mutable copy</returns>
    public Handle<Body<T>> Create(RectangleF rectangle, uint layer = 0, uint mask = 0, T userData = default) => _bodies.Add(new Body<T>
    {
        Bounds = rectangle,
        Layer = layer,
        Mask = mask,
        UserData = userData
    });

    /// <summary>
    /// Gets a reference to the stored body. Use with caution
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Body<T> Get(Handle<Body<T>> handle) => ref _bodies.Get(handle);

    /// <summary>
    /// Gets the UserData contained in the body mapped by <paramref name="handle"/>
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    public ref T GetUserData(Handle<Body<T>> handle) => ref _bodies.Get(handle).UserData;

    /// <summary>
    /// Sets the bitmask layer of the body
    /// Layer represents where the object is
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="layer"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetLayer(Handle<Body<T>> handle, uint layer) => _bodies[handle].Layer = layer;
    
    /// <summary>
    /// Sets the bitmask mask of the body
    /// Mask represents which objects this body collides with when moving
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="mask"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMask(Handle<Body<T>> handle, uint mask) => _bodies[handle].Mask = mask;

    /// <summary>
    /// Shorthand for <see cref="SetLayer(Handle{Body{T}}, uint)"/> and <see cref="SetMask(Handle{Body{T}}, uint)"/>
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="layer"></param>
    /// <param name="mask"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetLayerAndMask(Handle<Body<T>> handle, uint layer, uint mask)
    {
        SetLayer(handle, layer);
        SetMask(handle, mask);
    }


    /// <summary>
    /// Attemps to move the body associated with <paramref name="handle"/> to <paramref name="targetPosition"/>
    /// It uses <see cref="CollisionResponses.Stop(ref RectangleF, ref Vector2, CollisionInfo)"/> as solver
    /// This method is a shorthand of <see cref="Move(Handle{Body{T}}, Vector2, CollisionFilterFunction{T}, int, bool)"/>
    /// </summary>
    /// <param name="handle">Handle to the item to move</param>
    /// <param name="targetPosition">Desired position the body at <paramref name="handle"/> should move</param>
    /// <returns>True if there was a collision</returns>
    public bool Move(Handle<Body<T>> handle, Vector2 targetPosition) => Move(handle, targetPosition, CollisionFilters<T>.Response(CollisionResponses.Stop), 1);

    /// <summary>
    /// Attemps to move the body associated with <paramref name="handle"/> to <paramref name="targetPosition"/>
    /// </summary>
    /// <param name="handle">Handle to the item to move</param>
    /// <param name="targetPosition">Desired position the body at <paramref name="handle"/> should move</param>
    /// <param name="collisionFilter">Filter function that decides how each collision is handled</param>
    /// <param name="maxCollisions">Some <see cref="CollisionResponseFunction"/> need many iterations to be solved.
    /// This parameter limits the amount of iterations that can be done</param>
    /// <param name="testOnly">If <paramref name="testOnly"/> is true, the body does not move but the would-be collision information is given.</param>
    /// <returns>True if there was a collision</returns>
    public bool Move(Handle<Body<T>> handle, Vector2 targetPosition, CollisionFilterFunction<T> collisionFilter, int maxCollisions = 5, bool testOnly = false)
    {
        Debug.Assert(collisionFilter != null);
        Debug.Assert(_bodies.IsValid(handle));

        _lastCollisions.Clear();

        ref Body<T> body = ref _bodies[handle];

        RectangleF bounds = body.Bounds;
        Vector2 velocity = targetPosition - body.Bounds.Location;
        CollisionInfo lastCollision = CollisionInfo.NoCollision;

        for (int iteration = 0; iteration < maxCollisions; iteration++)
        {
            if (velocity.LengthSquared() <= float.Epsilon)
                break;

            CollisionInfo closestCollision = CollisionInfo.ValidFurthestCollision;
            Handle<Body<T>> lastCollisionHandle = Handle<Body<T>>.Default;

            foreach (HandleItem<Body<T>> item in _bodies)
            {
                if (item.Handle == handle)
                    continue;

                // Assymetric checks for now. Might make this a config setting in the future
                if ((body.Mask & item.Item.Layer) == 0 /*|| (item.Item.Mask & body.Layer) == 0*/)
                    continue;

                var response = CollisionFunctions.SweptAABB(bounds, item.Item.Bounds, velocity);

                if (response.HasCollision && response.CollisionTime < closestCollision.CollisionTime)
                {
                    closestCollision = response;
                    lastCollisionHandle = item.Handle;
                }
            }

            if (!closestCollision.HasCollision)
            {
                bounds.X += velocity.X;
                bounds.Y += velocity.Y;
                break;
            }

            lastCollision = closestCollision;
            _lastCollisions.Add(new(closestCollision, lastCollisionHandle));

            collisionFilter(ref Get(handle), ref Get(lastCollisionHandle))(ref bounds, ref velocity, closestCollision);
        }

        if (!testOnly)
            body.Bounds = bounds;

        return lastCollision.HasCollision;
    }

    /// <summary>
    /// Draws the bounds of the Bounds of the items in this <see cref="World{T}"/>
    /// </summary>
    /// <param name="spriteBatch"></param>
    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var item in _bodies)
            spriteBatch.Draw(item.Item.Bounds, Color.Red, 0.5f);
    }
}

/// <summary>
/// Struct that contains collision info and a handle to the items the collision happened with
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="collisionInfo"></param>
/// <param name="handle"></param>
public readonly struct CollisionHit<T>(CollisionInfo collisionInfo, Handle<T> handle)
{
    /// <summary>
    /// Normalized time in which the collision happened in the frame
    /// This value is invalid is <see cref="HasCollision"/> is false
    /// </summary>
    public readonly float CollisionTime { get; init; } = collisionInfo.CollisionTime;

    /// <summary>
    /// Remaining normalized time in the frame after the collision
    /// This value is invalid is <see cref="HasCollision"/> is false
    /// </summary>
    public readonly float RemaininTime => 1.0f - CollisionTime;

    /// <summary>
    /// Normal of the collision
    /// This value is invalid is <see cref="HasCollision"/> is false
    /// </summary>
    public readonly Vector2 Normal { get; init; } = collisionInfo.Normal;

    /// <summary>
    /// Handle to the object
    /// This value is invalid is <see cref="HasCollision"/> is false
    /// </summary>
    public readonly Handle<T> Handle = handle;

    /// <summary>
    /// Whether there was a collision
    /// </summary>
    public bool HasCollision { get; init; } = collisionInfo.HasCollision;
}
