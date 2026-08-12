using System;
using System.Collections.Generic;
using System.Diagnostics;
using Darkrit.Base;
using Darkrit.DataStructures;
using Darkrit.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RectangleF = Darkrit.Math.RectangleF;

namespace Darkrit.Physics.Boxy2D;

public class World
{
    private readonly HandleMapGrowing<RectangleF> _bodies = [];

    public Handle<RectangleF> Create(Vector2 center, Vector2 size)
    {
        return Create(new RectangleF
        {
            X = center.X - size.X * 0.5f,
            Y = center.Y - size.Y * 0.5f,
            Size = size
        });
    }

    public Handle<RectangleF> Create(RectangleF rectangle) => _bodies.Add(rectangle);

    public ref RectangleF Get(Handle<RectangleF> handle) => ref _bodies.Get(handle);

    public CollisionResponse Move(Handle<RectangleF> handle, Vector2 targetPosition)
    {
        Debug.Assert(_bodies.IsValid(handle));

        ref RectangleF body = ref _bodies.Get(handle);
        Vector2 velocity = targetPosition - body.Location;

        CollisionResponse closestCollision = new CollisionResponse
        {
            CollisionTime = float.PositiveInfinity
        };

        for (int i = 1; i < _bodies.Count; i++)
        {
            if (i == handle.Id || !_bodies.IsValid(handle))
                continue;

            RectangleF rect = _bodies[i];

            CollisionResponse response = CollisionFunctions.SweptAABB(body, rect, velocity);

            if (response.CollisionTime < closestCollision.CollisionTime)
                closestCollision = response;
        }

        if (closestCollision.HasCollision)
        {
            CollisionResponses.Stop(ref body, ref velocity, closestCollision);
            return closestCollision;
        }


        body.X = targetPosition.X;
        body.Y = targetPosition.Y;
        
        return closestCollision; // Invalid
    }

    public void InnerDraw(ref RectangleF rect)
    {
        _spriteBatch.Draw(rect, Color.Red, 0.5f);
    }

    SpriteBatch _spriteBatch;

    public void Draw(SpriteBatch spriteBatch)
    {
        this._spriteBatch = spriteBatch;
        _bodies.Iterate(InnerDraw);
    }
}
