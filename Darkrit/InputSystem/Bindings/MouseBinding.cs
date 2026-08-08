// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.InputSystem.Providers;

namespace Darkrit.InputSystem.Bindings;

/// <summary>
/// Bindings for mouse buttons
/// </summary>
public class MouseBinding(MouseButton button) : IInputBinding
{
    IInputProvider provider;
    IInputProvider IInputBinding.provider { set => provider = value; }
    public bool Pressed() => provider.IsMouseButtonDown(button);
    public bool Released() => provider.IsMouseButtonUp(button);
    public bool PressedThisFrame() => provider.WasMouseButtonJustPressed(button);
    public bool ReleasedThisFrame() => provider.WasMouseButtonJustReleased(button);

    public float GetValue() => Pressed() ? 1f : 0f;
}