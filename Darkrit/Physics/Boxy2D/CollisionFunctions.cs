using System;
using Microsoft.Xna.Framework;

using RectangleF = Darkrit.Math.RectangleF;

namespace Darkrit.Physics.Boxy2D;

public delegate void CollisionAction(
    ref RectangleF body,
    ref Vector2 velocity,
    CollisionResponse collisionResponse
);

public static class CollisionResponses
{
    public static void Push(ref RectangleF _, ref Vector2 velocity, CollisionResponse collisionResponde)
    {
        float magnitude = MathF.Sqrt((velocity.X * velocity.X + velocity.Y * velocity.Y)) * collisionResponde.RemaininTime;
        float dotprod = velocity.X * collisionResponde.Normal.Y + velocity.Y * collisionResponde.Normal.X;

        if (dotprod > 0.0f) dotprod = 1.0f;
        else if (dotprod < 0.0f) dotprod = -1.0f;

        velocity.X = dotprod * collisionResponde.Normal.Y * magnitude;
        velocity.Y = dotprod * collisionResponde.Normal.X * magnitude;
    }

    public static void Slide(ref RectangleF _, ref Vector2 velocity, CollisionResponse collisionResponde)
    {
        float dotprod = (velocity.X * collisionResponde.Normal.Y + velocity.Y * collisionResponde.Normal.X) * collisionResponde.RemaininTime;
        velocity.X = dotprod * collisionResponde.Normal.Y;
        velocity.Y = dotprod * collisionResponde.Normal.X;
    }

    public static void Stop(ref RectangleF r1, ref Vector2 velocity, CollisionResponse collisionResponde)
    {
        r1.X += velocity.X * collisionResponde.CollisionTime;
        r1.Y += velocity.Y * collisionResponde.CollisionTime;
    }
}

public readonly struct CollisionResponse
{
    public readonly float CollisionTime { get; init; }

    public readonly float RemaininTime => 1.0f - CollisionTime;


    public readonly Vector2 Normal { get; init; }

    public static CollisionResponse Invalid = new() { CollisionTime = -1 };

    public CollisionResponse()
    {
        CollisionTime = -1.0f;
    }

    public bool HasCollision => CollisionTime >= 0.0f && CollisionTime <= 1.0f;
}

public class CollisionFunctions
{
    static float SweptAABB(RectangleF r1, RectangleF r2, Vector2 delta, out Vector2 normal)
    {
        float xInvEntry, yInvEntry;
        float xInvExit, yInvExit;

        // Find the distance between the objects on the near and far sides for both x and y
        if(delta.X > 0.0f)
        {
            xInvEntry = r2.X - (r1.X + r1.Width);
            xInvExit = (r2.X + r2.Width) - r1.X;
        }
        else
        {
            xInvEntry = (r2.X + r2.Width) - r1.X;
            xInvExit = r2.X - (r1.X + r1.Width);
        }

        if (delta.Y > 0.0f)
        {
            yInvEntry = r2.Y - (r1.Y + r1.Height);
            yInvExit = (r2.Y + r2.Height) - r1.Y;
        }
        else
        {
            yInvEntry = (r2.Y + r2.Height) - r1.Y;
            yInvExit = r2.Y - (r1.Y + r1.Height);
        }

        // Find time of collision and time of leaving for each axis (if statement is to prevent divide by zero)
        float xEntry, yEntry;
        float xExit, yExit;

        if(delta.X == 0.0f)
        {
            xEntry = float.NegativeInfinity;
            xExit = float.PositiveInfinity;
        }
        else
        {
            xEntry = xInvEntry / delta.X;
            xExit = xInvExit / delta.X;
        }

        if (delta.Y == 0.0f)
        {
            yEntry = float.NegativeInfinity;
            yExit = float.PositiveInfinity;
        }
        else
        {
            yEntry = yInvEntry / delta.Y;
            yExit = yInvExit / delta.Y;
        }

        // Find the earliest time of collision
        float entryTime = SMath.Max(xEntry, yEntry);
        float exitTime = SMath.Min(xExit, yExit);

        // If here was no collision
        if (entryTime > exitTime || (xEntry < 0.0f && yEntry < 0.0f) || xEntry > 1.0f || yEntry > 1.0f)
        {
            normal = Vector2.Zero;
            return -1.0f;
        }
        // There was a collision
        else
        {
            if (xEntry > yEntry)
                normal = new Vector2(xInvEntry < 0.0f ? 1.0f : -1.0f, 0.0f);
            else
                normal = new Vector2(0.0f, yInvEntry < 0.0f ? 1.0f : -1.0f);

            return entryTime;
        }
    }

    public static CollisionResponse SweeptAABB(RectangleF r1, RectangleF r2, Vector2 delta)
    {
        float collisiontime = SweptAABB(r1, r2, delta, out Vector2 normal);

        if (collisiontime < 0.0f)
            return CollisionResponse.Invalid;
        
        r1.X += delta.X * collisiontime;
        r1.Y += delta.Y * collisiontime;

        return new CollisionResponse
        {
            CollisionTime = collisiontime,
            Normal = normal
        };
    }
}
