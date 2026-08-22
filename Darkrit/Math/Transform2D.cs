// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;

namespace Darkrit.Math;

/// <summary>
/// 2D non affine transform that provides Position, Scale and Rotation
/// </summary>
public struct Transform2D
{
    /// <summary>
    /// Position in space
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// Scale of the item
    /// </summary>
    public Vector2 Scale;

    /// <summary>
    /// Rotation (in radians)
    /// </summary>
    public float Rotation;
}
