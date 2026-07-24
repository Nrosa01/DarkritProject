using Darkrit.Input;
using System;
using System.Collections.Generic;
using System.Text;

using Darkrit.Input.Providers;

namespace Darkrit.Input.Bindings;

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