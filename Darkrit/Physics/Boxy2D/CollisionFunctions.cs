using System;
using Microsoft.Xna.Framework;

using RectangleF = Darkrit.Math.RectangleF;

namespace Darkrit.Physics.Boxy2D;

public delegate bool CollisionResponseFunction(ref RectangleF body, ref Vector2 velocity, CollisionInfo collisionResponse);

public class CollisionFunctions
{
    public static CollisionInfo SweptAABB(RectangleF r1, RectangleF r2, Vector2 motion)
    {
        Vector2 halfSize = new(
            r1.Width * 0.5f,
            r1.Height * 0.5f
        );

        Vector2 center = new(
            r1.Left + halfSize.X,
            r1.Top + halfSize.Y
        );

        // Expands r2 by the size of r1
        // That allows to treat r1 as if it was a point
        RectangleF expanded = new(
            r2.Left - halfSize.X,
            r2.Top - halfSize.Y,
            r2.Width + r1.Width,
            r2.Height + r1.Height
        );

        float lastEntry = float.NegativeInfinity;
        float firstExit = float.PositiveInfinity;

        // X e Y
        for (int axis = 0; axis < 2; axis++)
        {
            float pos = axis == 0 ? center.X : center.Y;
            float movement = axis == 0 ? motion.X : motion.Y;

            float min = axis == 0 ? expanded.Left : expanded.Top;
            float max = axis == 0 ? expanded.Right : expanded.Bottom;

            if (movement != 0.0f)
            {
                float t1 = (min - pos) / movement;
                float t2 = (max - pos) / movement;

                lastEntry = MathF.Max(lastEntry,MathF.Min(t1, t2));

                firstExit = MathF.Min(firstExit,MathF.Max(t1, t2));
            }
            else
            {
                // We don't move in this axis.
                // If the center is outside the interval,
                // there will never be a point in the extended AABB
                if (pos <= min || pos >= max)
                    return CollisionInfo.NoCollision;
            }
        }

        // There is no intersection with the segmnet [0, 1]
        if (firstExit <= lastEntry || firstExit <= 0.0f || lastEntry >= 1.0f)
            return CollisionInfo.NoCollision;

        Vector2 hitPosition = center + motion * lastEntry;

        // Normal
        float dx = hitPosition.X - expanded.Left - expanded.Width * 0.5f;
        float dy = hitPosition.Y - expanded.Top - expanded.Height * 0.5f;

        float px = expanded.Width * 0.5f - MathF.Abs(dx);
        float py = expanded.Height * 0.5f - MathF.Abs(dy);

        Vector2 normal;

        if (px < py)
            normal = new Vector2(dx > 0 ? 1.0f : -1.0f, 0.0f);
        else
            normal = new Vector2(0.0f, dy > 0 ? 1.0f : -1.0f);

        return new CollisionInfo
        {
            HasCollision = true,
            CollisionTime = lastEntry,
            Normal = normal
        };
    }
}

public static class CollisionResponses
{
    public static bool Push(ref RectangleF _, ref Vector2 velocity, CollisionInfo collisionResponde)
    {
        float magnitude = MathF.Sqrt((velocity.X * velocity.X + velocity.Y * velocity.Y)) * collisionResponde.RemaininTime;
        float dotprod = velocity.X * collisionResponde.Normal.Y + velocity.Y * collisionResponde.Normal.X;

        if (dotprod > 0.0f) dotprod = 1.0f;
        else if (dotprod < 0.0f) dotprod = -1.0f;

        velocity.X = dotprod * collisionResponde.Normal.Y * magnitude;
        velocity.Y = dotprod * collisionResponde.Normal.X * magnitude;

        return true;
    }

    public static bool Slide(ref RectangleF body, ref Vector2 velocity, CollisionInfo response)
    {
        body.X += velocity.X * response.CollisionTime;
        body.Y += velocity.Y * response.CollisionTime;

        velocity *= response.RemaininTime;

        float normalVelocity = Vector2.Dot(velocity, response.Normal);

        if (normalVelocity < 0.0f)
            velocity -= response.Normal * normalVelocity;

        return true;
    }

    public static bool Stop(ref RectangleF r1, ref Vector2 velocity, CollisionInfo collisionResponde)
    {
        r1.X += velocity.X * collisionResponde.CollisionTime;
        r1.Y += velocity.Y * collisionResponde.CollisionTime;

        velocity = Vector2.Zero;
        return false;
    }

    public static bool Cross(ref RectangleF body, ref Vector2 velocity, CollisionInfo collisionResponse)
    {
        body.X += velocity.X;
        body.Y += velocity.Y;
        return false;
    }
}

