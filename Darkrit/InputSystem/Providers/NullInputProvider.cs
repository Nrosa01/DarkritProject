using Darkrit.InputSystem.Bindings;
using Darkrit.InputSystem.ReplaySystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Darkrit.InputSystem.Providers;

/// <summary>
/// Input provider that returns false or default values. Useful for ignoring all inputs.
/// </summary>
public class NullInputProvider : ISerializableInputProvider
{
    public Point LastRecordedMousePosition { get; set; }

    public void Update(GameTime gameTime)
    {
    }

    // ===== Keyboard =====
    public bool IsKeyDown(Keys key) => false;
    public bool IsKeyUp(Keys key) => false;
    public bool WasKeyJustPressed(Keys key) => false;
    public bool WasKeyJustReleased(Keys key) => false;
    public Keys[] GetPressedKeys() => [];

    // ===== Mouse =====
    public Point GetMousePosition() => LastRecordedMousePosition;
    public int GetMouseScrollWheelValue() => default;
    public bool IsMouseButtonDown(MouseButton button) => false;
    public bool IsMouseButtonUp(MouseButton button) => false;
    public bool WasMouseButtonJustPressed(MouseButton button) => false;
    public bool WasMouseButtonJustReleased(MouseButton button) => false;
    public Point GetMousePositionDelta() => default;

    // ===== Gamepad =====
    public bool IsGamepadConnected(PlayerIndex playerIndex) => false;
    public bool IsGamepadButtonDown(PlayerIndex playerIndex, Buttons button) => false;
    public bool IsGamepadButtonUp(PlayerIndex playerIndex, Buttons button) => false;
    public bool WasGamepadButtonJustPressed(PlayerIndex playerIndex, Buttons button) => false;
    public bool WasGamepadButtonJustReleased(PlayerIndex playerIndex, Buttons button) => false;
    public Vector2 GetGamepadLeftStick(PlayerIndex playerIndex) => default;
    public Vector2 GetGamepadRightStick(PlayerIndex playerIndex) => default;
    public float GetGamepadLeftTrigger(PlayerIndex playerIndex) => default;
    public float GetGamepadRightTrigger(PlayerIndex playerIndex) => default;
    public float GetGamepadAxis(PlayerIndex playerIndex, GamepadAxis axis) => default;

    internal InputFrame CaptureFrame()
    {
        return new InputFrame
        {
            GamePads = [new GamePadFrame {

            }],
            Keyboard = new KeyboardFrame
            {

            },
            Mouse = new MouseFrame
            {
                Position = new(LastRecordedMousePosition.X, LastRecordedMousePosition.Y)
            }
        };
    }

    InputFrame ISerializableInputProvider.CaptureFrame() => CaptureFrame();
}