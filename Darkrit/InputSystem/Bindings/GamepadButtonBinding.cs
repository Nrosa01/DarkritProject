// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.InputSystem.Providers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Darkrit.InputSystem.Bindings;

/// <summary>
/// Bindings for gamepad buttons
/// </summary>
internal class GamepadButtonBinding(PlayerIndex playerIndex, Buttons button) : IInputBinding
{
    IInputProvider provider;

    IInputProvider IInputBinding.provider { set => provider = value; }

    public GamepadButtonBinding(Buttons button)
        : this(PlayerIndex.One, button) { }

    public bool Pressed() => provider.IsGamepadButtonDown(playerIndex, button);
    public bool Released() => provider.IsGamepadButtonUp(playerIndex, button);
    public bool PressedThisFrame() => provider.WasGamepadButtonJustPressed(playerIndex, button);
    public bool ReleasedThisFrame() => provider.WasGamepadButtonJustReleased(playerIndex, button);

    public float GetValue() => Pressed() ? 1f : 0f;
}