// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Darkrit.InputSystem.ReplaySystem;

/// <summary>
/// Defines all the input state that exists in a frame
/// </summary>
public readonly struct InputFrame
{
    public readonly KeyboardFrame Keyboard { get; init; }
    public readonly MouseFrame Mouse { get; init; }
    public readonly GamePadFrame[] GamePads { get; init; }
}
