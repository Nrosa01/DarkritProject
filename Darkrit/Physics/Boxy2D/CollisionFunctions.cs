using System;
using System.Runtime.CompilerServices;
using Darkrit.Math;
using Microsoft.Xna.Framework;

namespace Darkrit.Physics.Boxy2D;

public delegate void CollisionAction(
    ref RectangleF body,
    ref Vector2 velocity,
    float remainingTime,
    Vector2 normal
);

public static class CollisionResponses
{
    public static void Push(ref RectangleF _, ref Vector2 velocity, float remainingTime, Vector2 normal)
    {
        float magnitude = MathF.Sqrt((velocity.X * velocity.X + velocity.Y * velocity.Y)) * remainingTime;
        float dotprod = velocity.X * normal.Y + velocity.Y * normal.X;

        if (dotprod > 0.0f) dotprod = 1.0f;
        else if (dotprod < 0.0f) dotprod = -1.0f;

        velocity.X = dotprod * normal.Y * magnitude;
        velocity.Y = dotprod * normal.X * magnitude;
    }

    public static void Slide(ref RectangleF _, ref Vector2 velocity, float remainingTime, Vector2 normal)
    {
        float dotprod = (velocity.X * normal.Y + velocity.Y * normal.X) * remainingTime;
        velocity.X = dotprod * normal.Y;
        velocity.Y = dotprod * normal.X;
    }

    public static void None(ref RectangleF _, ref Vector2 _1, float _2, Vector2 _3) { }
}

public readonly struct CollisionResponse
{
    public readonly float CollisionTime { get; init; }
    public readonly Vector2 Normal { get; init; }
}

public class CollisionFunctions
{
    static float SweptAABB(RectangleF r1, RectangleF r2, Vector2 velocity, out Vector2 normal)
    {
        float xInvEntry, yInvEntry;
        float xInvExit, yInvExit;

        // Find the distance between the objects on the near and far sides for both x and y
        if(velocity.X > 0.0f)
        {
            xInvEntry = r2.X - (r1.X + r1.Width);
            xInvExit = (r2.X + r2.Width) - r1.X;
        }
        else
        {
            xInvEntry = (r2.X + r2.Width) - r1.X;
            xInvExit = r2.X - (r1.X + r1.Width);
        }

        if (velocity.Y > 0.0f)
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

        if(velocity.X == 0.0f)
        {
            xEntry = float.NegativeInfinity;
            xExit = float.PositiveInfinity;
        }
        else
        {
            xEntry = xInvEntry / velocity.X;
            xExit = xInvExit / velocity.X;
        }

        if (velocity.Y == 0.0f)
        {
            yEntry = float.NegativeInfinity;
            yExit = float.PositiveInfinity;
        }
        else
        {
            yEntry = yInvEntry / velocity.Y;
            yExit = yInvExit / velocity.Y;
        }

        // Find the earliest time of collision
        float entryTime = SMath.Max(xEntry, yEntry);
        float exitTime = SMath.Min(xExit, yExit);

        // If here was no collision
        if (entryTime > exitTime || xEntry < 0.0f && yEntry < 0.0f || xEntry > 1.0f || yEntry > 1.0f)
        {
            normal = Vector2.Zero;
            return -1.0f;
        }
        // There was a collision
        else
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            float InvertedSignF(float t) => xInvEntry < 0.0f ? 1.0f : -1.0f;

            // Calculate normal of collided surface
            if (xEntry > yEntry)
                normal = new Vector2(InvertedSignF(xInvEntry), 0);
            else
                normal = new Vector2(0, InvertedSignF(yInvEntry));

            return entryTime;
        }
    }

    static CollisionResponse SweeptAABBWithResponse(RectangleF r1, RectangleF r2, ref Vector2 velocity) => SweeptAABBWithResponse(r1, r2, ref velocity, CollisionResponses.None);

    static CollisionResponse SweeptAABBWithResponse(RectangleF r1, RectangleF r2, ref Vector2 velocity, CollisionAction collisionAction)
    {
        float collisiontime = SweptAABB(r1, r2, velocity, out Vector2 normal);

        if (collisiontime < 0.0f)
            return new CollisionResponse
            {
                CollisionTime = -1.0f
            };
        
        r1.X += velocity.X * collisiontime;
        r1.Y += velocity.Y * collisiontime;
        float remainingtime = 1.0f - collisiontime;

        collisionAction(ref r1, ref velocity, remainingtime, normal);

        return new CollisionResponse
        {
            CollisionTime = collisiontime,
            Normal = normal
        };
    }
}
