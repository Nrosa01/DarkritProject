using Microsoft.Xna.Framework;

namespace Darkrit.Physics.Boxy2D;

public readonly struct CollisionInfo
{
    public readonly float CollisionTime { get; init; }

    public readonly float RemaininTime => 1.0f - CollisionTime;

    public readonly Vector2 Normal { get; init; }

    public readonly static CollisionInfo NoCollision = new() { HasCollision = false, CollisionTime = -1.0f };
    public readonly static CollisionInfo ValidFurthestCollision = new() { HasCollision = false, CollisionTime = float.PositiveInfinity };

    public CollisionInfo()
    {
    }

    public bool HasCollision { get; init; } = false;
}

