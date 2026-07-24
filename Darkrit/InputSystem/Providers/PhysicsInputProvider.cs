using Darkrit.InputSystem.Bindings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Darkrit.InputSystem;

namespace Darkrit.InputSystem.Providers;

/// <summary>
/// Input provider that reads input data from hardware devices.
/// </summary>
public class PhysicalInputProvider : IInputProvider
{
    private readonly KeyboardInfo _keyboard;
    private readonly MouseInfo _mouse;
    private readonly GamePadInfo[] _gamePads;

    public PhysicalInputProvider()
    {
        _keyboard = new KeyboardInfo();
        _mouse = new MouseInfo();
        _gamePads = new GamePadInfo[4];
        for (int i = 0; i < 4; i++)
            _gamePads[i] = new GamePadInfo((PlayerIndex)i);
    }

    public void Update(GameTime gameTime)
    {
        _keyboard.Update();
        _mouse.Update();
        for (int i = 0; i < 4; i++)
            _gamePads[i].Update(gameTime);
    }

    // ===== Keyboard =====
    public bool IsKeyDown(Keys key) => _keyboard.IsKeyDown(key);
    public bool IsKeyUp(Keys key) => _keyboard.IsKeyUp(key);
    public bool WasKeyJustPressed(Keys key) => _keyboard.WasKeyJustPressed(key);
    public bool WasKeyJustReleased(Keys key) => _keyboard.WasKeyJustReleased(key);
    public Keys[] GetPressedKeys() => _keyboard.CurrentState.GetPressedKeys();

    // ===== Mouse =====
    public Point GetMousePosition() => _mouse.Position;
    public int GetMouseScrollWheelValue() => _mouse.CurrentState.ScrollWheelValue;
    public bool IsMouseButtonDown(MouseButton button) => _mouse.IsButtonDown(button);
    public bool IsMouseButtonUp(MouseButton button) => _mouse.IsButtonUp(button);
    public bool WasMouseButtonJustPressed(MouseButton button) => _mouse.WasButtonJustPressed(button);
    public bool WasMouseButtonJustReleased(MouseButton button) => _mouse.WasButtonJustReleased(button);
    public Point GetMousePositionDelta() => _mouse.PositionDelta;

    // ===== Gamepad =====
    public bool IsGamepadConnected(PlayerIndex playerIndex) => _gamePads[(int)playerIndex].IsConnected;
    public bool IsGamepadButtonDown(PlayerIndex playerIndex, Buttons button) => _gamePads[(int)playerIndex].IsButtonDown(button);
    public bool IsGamepadButtonUp(PlayerIndex playerIndex, Buttons button) => _gamePads[(int)playerIndex].IsButtonUp(button);
    public bool WasGamepadButtonJustPressed(PlayerIndex playerIndex, Buttons button) => _gamePads[(int)playerIndex].WasButtonJustPressed(button);
    public bool WasGamepadButtonJustReleased(PlayerIndex playerIndex, Buttons button) => _gamePads[(int)playerIndex].WasButtonJustReleased(button);
    public Vector2 GetGamepadLeftStick(PlayerIndex playerIndex) => _gamePads[(int)playerIndex].LeftThumbStick;
    public Vector2 GetGamepadRightStick(PlayerIndex playerIndex) => _gamePads[(int)playerIndex].RightThumbStick;
    public float GetGamepadLeftTrigger(PlayerIndex playerIndex) => _gamePads[(int)playerIndex].LeftTrigger;
    public float GetGamepadRightTrigger(PlayerIndex playerIndex) => _gamePads[(int)playerIndex].RightTrigger;

    public float GetGamepadAxis(PlayerIndex playerIndex, GamepadAxis axis)
    {
        var gp = _gamePads[(int)playerIndex];
        return axis switch
        {
            GamepadAxis.LeftStickX => gp.LeftThumbStick.X,
            GamepadAxis.LeftStickY => gp.LeftThumbStick.Y,
            GamepadAxis.RightStickX => gp.RightThumbStick.X,
            GamepadAxis.RightStickY => gp.RightThumbStick.Y,
            GamepadAxis.LeftTrigger => gp.LeftTrigger,
            GamepadAxis.RightTrigger => gp.RightTrigger,
            _ => 0f
        };
    }
}