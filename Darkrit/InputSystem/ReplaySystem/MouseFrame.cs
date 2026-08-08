// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Drawing;

namespace Darkrit.InputSystem.ReplaySystem;

/// <summary>
/// Stores mouse frame data info
/// </summary>
public readonly struct MouseFrame
{
    public readonly Point Position { get; init; }
    public readonly int Wheel { get; init; }

    public readonly bool Left { get; init; }
    public readonly bool Middle { get; init; }
    public readonly bool Right { get; init; }
    public readonly bool X1 { get; init; } // First mouse lateral button
    public readonly bool X2 { get; init; } // Second mouse lateral button
}
