using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Darkrit.Base;
using Darkrit.DataStructures;
using Darkrit.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RectangleF = Darkrit.Math.RectangleF;

namespace Darkrit.Physics.Boxy2D;

public delegate CollisionAction CollisionFilter<T>(ref Body<T> self, ref Body<T> other);

public static  class Test {
    public static CollisionAction Stop<T>(ref Body<T> self, ref Body<T> other) => CollisionResponses.Stop;
    public static CollisionAction Slide<T>(ref Body<T> self, ref Body<T> other) => CollisionResponses.Slide;
}

public class World<T>
{
    private readonly HandleMapGrowing<Body<T>> _bodies = [];
    private readonly GrowableArray<CollisionHit<Body<T>>> _lastCollisions = [];

    public ReadOnlySpan<CollisionHit<Body<T>>> LastCollsions => _lastCollisions.AsReadOnlySpan();

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

    public Handle<Body<T>> Create(RectangleF rectangle, uint layer = 0, uint mask = 0, T userData = default) => _bodies.Add(new Body<T>
    {
        Bounds = rectangle,
        Layer = layer,
        Mask = mask,
        UserData = userData
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Body<T> Get(Handle<Body<T>> handle) => ref _bodies.Get(handle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetLayer(Handle<Body<T>> handle, uint layer) => _bodies[handle].Layer = layer;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMask(Handle<Body<T>> handle, uint mask) => _bodies[handle].Mask = mask;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetLayerAndMask(Handle<Body<T>> handle, uint layer, uint mask)
    {
        SetLayer(handle, layer);
        SetMask(handle, mask);
    }

    public bool Move(Handle<Body<T>> handle, Vector2 targetPosition) => Move(handle, targetPosition, Test.Stop, 1);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="targetPosition"></param>
    /// <param name="collisionAction"></param>
    /// <param name="maxCollisions"></param>
    /// <param name="testOnly">If <paramref name="testOnly"/> is true, the body does not move but the would-be collision information is given.</param>
    /// <returns></returns>
    public bool Move(Handle<Body<T>> handle, Vector2 targetPosition, CollisionFilter<T> collisionFilter, int maxCollisions = 5, bool testOnly = false)
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

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var item in _bodies)
            spriteBatch.Draw(item.Item.Bounds, Color.Red, 0.5f);
    }
}

public readonly struct CollisionHit<T>(CollisionInfo collisionInfo, Handle<T> handle)
{
    public readonly float CollisionTime { get; init; } = collisionInfo.CollisionTime;

    public readonly float RemaininTime => 1.0f - CollisionTime;

    public readonly Vector2 Normal { get; init; } = collisionInfo.Normal;

    public readonly Handle<T> Handle = handle;

    public bool HasCollision { get; init; } = collisionInfo.HasCollision;
}
