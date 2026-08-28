using Darkrit.Base;
using Darkrit.Physics.Boxy2D;
using Darkrit.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using RectangleF = Darkrit.Math.RectangleF;

namespace Darkrit.EntityModel.Components;

[Component]
public partial struct Area2D
{
    [SerializeField] Vector2 size = Vector2.One * 24;
    [SerializeField] Vector2 offset;

    public bool IsMonitoring;

    HashSet<Handle<Body<Handle<PhysicsBody>>>> _bodiesInside;

    public event Action<Handle<Body<Handle<PhysicsBody>>>> BodyEntered;
    public event Action<Handle<Body<Handle<PhysicsBody>>>> BodyStayed;
    public event Action<Handle<Body<Handle<PhysicsBody>>>> BodyExited;

    public Vector2 Size
    {
        readonly get => size;
        set => size = value;
    }

    public Vector2 Offset
    {
        readonly get => offset;
        set => offset = value;
    }

    public readonly bool Contains(Vector2 point) => GetBounds().Contains(point.X, point.Y);

    public readonly bool Overlaps(RectangleF rect) => GetBounds().Intersects(rect);


    public readonly bool Contains(Handle<Body<Handle<PhysicsBody>>> body) => Overlaps(World.Physics.Get(body).Bounds);

    public void OnAdd()
    {
        _bodiesInside = [];
    }

    public void FixedUpdate(GameTime gameTime)
    {
        if (!IsMonitoring)
            return;

        HashSet<Handle<Body<Handle<PhysicsBody>>>> current = [];

        foreach (ref PhysicsBody physicsBody in World.PhysicsBodyStore)
        {
            if (physicsBody.Body.Handle.Id == 0)
                continue;

            if (!Overlaps(physicsBody.Body.Bounds))
                continue;

            Handle<Body<Handle<PhysicsBody>>> handle = physicsBody.Body.Handle;
            current.Add(handle);

            if (_bodiesInside.Contains(handle))
                BodyStayed?.Invoke(handle);
            else
                BodyEntered?.Invoke(handle);
        }

        foreach (Handle<Body<Handle<PhysicsBody>>> handle in _bodiesInside)
        {
            if (!current.Contains(handle))
            { 
                ref var body = ref World.Physics.Get(handle);
                ref var physicsBody = ref World.PhysicsBodyStore.Get(body.UserData);
                BodyExited?.Invoke(handle);
            }
        }

        _bodiesInside = current;
    }

    public void Draw(GameTime gameTime)
    {
        Core.SpriteBatch.Draw(GetBounds(), Color.Yellow, 0.5f);
    }

    readonly RectangleF GetBounds()
    {
        Vector2 center = Entity.Position + offset;

        return new RectangleF
        {
            X = center.X - size.X * 0.5f,
            Y = center.Y - size.Y * 0.5f,
            Size = size
        };
    }
}