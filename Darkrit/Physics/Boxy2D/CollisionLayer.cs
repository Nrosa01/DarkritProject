using System;

namespace Darkrit.Physics.Boxy2D;

[Flags]
public enum CollisionLayer : uint
{
    None = 0,
    World = 1 << 0,
    Player = 1 << 1,
    Enemy = 1 << 2,
    Projectile = 1 << 3
}