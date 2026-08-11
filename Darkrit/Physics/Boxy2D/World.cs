using System;
using System.Collections.Generic;
using System.Diagnostics;
using Darkrit.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RectangleF = Darkrit.Math.RectangleF;


namespace Darkrit.Physics.Boxy2D;

using Handle = HandleMapGrowing<RectangleF>.Handle;

public class World
{
    private readonly HandleMapGrowing<RectangleF> _bodies = new();

    public Handle Create(Vector2 center, Vector2 size)
    {
        return Create(new RectangleF
        {
            X = center.X - size.X * 0.5f,
            Y = center.Y - size.Y * 0.5f,
            Size = size
        });
    }

    public Handle Create(RectangleF rectangle) => _bodies.Add(rectangle);

    public ref RectangleF Get(Handle handle) => ref _bodies.Get(handle);

    public CollisionResponse Move(Handle handle, Vector2 targetPosition)
    {
        Debug.Assert(_bodies.Exists(handle));

        ref RectangleF body = ref _bodies.Get(handle);
        Vector2 velocity = targetPosition - body.Location;

        CollisionResponse closestCollision = new CollisionResponse
        {
            CollisionTime = float.PositiveInfinity
        };

        for (int i = 1; i < _bodies.Items.Count; i++)
        {
            if (i == handle.Id)
                continue;

            RectangleF rect = _bodies.Items[i];

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

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < _bodies.Items.Count; i++)
        {
            RectangleF rect = _bodies.Items[i];

            spriteBatch.Draw(rect, Color.Red, 0.5f);
        }
    }
}
