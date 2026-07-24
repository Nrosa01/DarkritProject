using Darkrit.InputSystem.Providers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Darkrit.InputSystem.Bindings;

/// <summary>
/// Bindings for Gamepad buttons and axis
/// </summary>
public class GamepadBinding : IInputBinding
{
    IInputBinding _inputBinding;

    IInputProvider IInputBinding.provider { set => _inputBinding.provider = value; }
    public GamepadBinding(PlayerIndex playerIndex, GamepadAxis axis, float deadZone = 0.2f)
        => _inputBinding = new GamepadAxisBinding(playerIndex, axis, deadZone);

    public GamepadBinding(GamepadAxis axis, float deadZone = 0.2f) => _inputBinding = new GamepadAxisBinding(axis, deadZone);

    public GamepadBinding(PlayerIndex playerIndex, Buttons button) => _inputBinding = new GamepadButtonBinding(playerIndex, button);

    public GamepadBinding(Buttons button) => _inputBinding = new GamepadButtonBinding(button);

    public float GetValue() => _inputBinding.GetValue();

    public bool Pressed() => _inputBinding.Pressed();

    public bool PressedThisFrame() => _inputBinding.PressedThisFrame();

    public bool ReleasedThisFrame() => _inputBinding.ReleasedThisFrame();
}
