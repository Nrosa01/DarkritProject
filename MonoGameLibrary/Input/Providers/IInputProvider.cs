using Darkrit.Input.Bindings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Input;

namespace Darkrit.Input.Providers;

public interface IInputProvider
{
    // Keyboard
    bool IsKeyDown(Keys key);
    bool IsKeyUp(Keys key);
    bool WasKeyJustPressed(Keys key);
    bool WasKeyJustReleased(Keys key);
    Keys[] GetPressedKeys();

    // Mouse
    Point GetMousePosition();
    int GetMouseScrollWheelValue();
    bool IsMouseButtonDown(MouseButton button);
    bool IsMouseButtonUp(MouseButton button);
    bool WasMouseButtonJustPressed(MouseButton button);
    bool WasMouseButtonJustReleased(MouseButton button);
    Point GetMousePositionDelta();

    // Gamepad
    bool IsGamepadConnected(PlayerIndex playerIndex);
    bool IsGamepadButtonDown(PlayerIndex playerIndex, Buttons button);
    bool IsGamepadButtonUp(PlayerIndex playerIndex, Buttons button);
    bool WasGamepadButtonJustPressed(PlayerIndex playerIndex, Buttons button);
    bool WasGamepadButtonJustReleased(PlayerIndex playerIndex, Buttons button);
    Vector2 GetGamepadLeftStick(PlayerIndex playerIndex);
    Vector2 GetGamepadRightStick(PlayerIndex playerIndex);
    float GetGamepadLeftTrigger(PlayerIndex playerIndex);
    float GetGamepadRightTrigger(PlayerIndex playerIndex);
    float GetGamepadAxis(PlayerIndex playerIndex, GamepadAxis axis);

    // General
    void Update(GameTime gameTime);
}
