using Darkrit.InputSystem.Providers;
using Microsoft.Xna.Framework.Input;

namespace Darkrit.InputSystem.Bindings;

/// <summary>
/// Bindings for keyboard keys
/// </summary>
/// <param name="key"></param>
/// <param name="provider"></param>
public class KeyboardBinding(Keys key) : IInputBinding
{
    IInputProvider provider;

    IInputProvider IInputBinding.provider { set => provider = value; }

    public bool Pressed() => provider.IsKeyDown(key);
    public bool PressedThisFrame() => provider.WasKeyJustPressed(key);
    public bool ReleasedThisFrame() => provider.WasKeyJustReleased(key);
    public float GetValue() => Pressed() ? 1f : 0f;
}
