// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.InputSystem.Bindings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Darkrit.InputSystem.Providers;

/// <summary>
/// Basic interface for input handling
/// </summary>
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
