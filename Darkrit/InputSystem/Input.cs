using Darkrit.InputSystem.Bindings;
using Darkrit.InputSystem.Providers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Darkrit.InputSystem;

/// <summary>
/// Main input system. Offers a unified API to check keyboard, mouse
/// and gamepad. 
/// </summary>
/// <remarks>
/// Creates a new Input instance that uses a certain provider
/// </remarks>
/// <param name="provider">Input provider to use.</param>
public class Input(IInputProvider provider) : IInputProvider
{
    private InputMap _actionMap = new();

    /// <summary>
    /// Action map that maps string names to bindings
    /// </summary>
    public InputMap ActionMap => _actionMap;

    /// <summary>
    /// Current hardware provider
    /// </summary>
    public IInputProvider Provider => provider;

    /// <summary>
    /// Creates a new instance using the physics input provider by default
    /// </summary>
    public Input() : this(new ActivatableInputProvider()) { }

    public InputAction CreateAction(string actionName) => _actionMap.AddAction(actionName, this);

    /// <summary>
    /// Changes the input provider in runtime.
    /// This is mainly thought for using a ReplayInputProvider
    /// </summary>
    public void SetProvider(IInputProvider newProvider) => provider = newProvider;

    /// <summary>
    /// Updates all input states.
    /// Must be called once per frame.
    /// </summary>
    public void Update(GameTime gameTime) => provider.Update(gameTime);
    // ===== Direct access methods for when you're lazy or prototyping =====

    // TODO: It's a chore, but I should add stupid xml doc for these helpers

    // Keyboard
    public bool IsKeyDown(Keys key) => provider.IsKeyDown(key);
    public bool IsKeyUp(Keys key) => provider.IsKeyUp(key);
    public bool WasKeyJustPressed(Keys key) => provider.WasKeyJustPressed(key);
    public bool WasKeyJustReleased(Keys key) => provider.WasKeyJustReleased(key);
    public Keys[] GetPressedKeys() => provider.GetPressedKeys();

    // Mouse
    public Point GetMousePosition() => provider.GetMousePosition();
    public int GetMouseScrollWheelValue() => provider.GetMouseScrollWheelValue();
    public bool IsMouseButtonDown(MouseButton button) => provider.IsMouseButtonDown(button);
    public bool IsMouseButtonUp(MouseButton button) => provider.IsMouseButtonUp(button);
    public bool WasMouseButtonJustPressed(MouseButton button) => provider.WasMouseButtonJustPressed(button);
    public bool WasMouseButtonJustReleased(MouseButton button) => provider.WasMouseButtonJustReleased(button);
    public Point GetMousePositionDelta() => provider.GetMousePositionDelta();

    // Gamepad
    public bool IsGamepadConnected(PlayerIndex playerIndex) => provider.IsGamepadConnected(playerIndex);
    public bool IsGamepadButtonDown(PlayerIndex playerIndex, Buttons button) => provider.IsGamepadButtonDown(playerIndex, button);
    public bool IsGamepadButtonUp(PlayerIndex playerIndex, Buttons button) => provider.IsGamepadButtonUp(playerIndex, button);
    public bool WasGamepadButtonJustPressed(PlayerIndex playerIndex, Buttons button) => provider.WasGamepadButtonJustPressed(playerIndex, button);
    public bool WasGamepadButtonJustReleased(PlayerIndex playerIndex, Buttons button) => provider.WasGamepadButtonJustReleased(playerIndex, button);
    public Vector2 GetGamepadLeftStick(PlayerIndex playerIndex) => provider.GetGamepadLeftStick(playerIndex);
    public Vector2 GetGamepadRightStick(PlayerIndex playerIndex) => provider.GetGamepadRightStick(playerIndex);
    public float GetGamepadLeftTrigger(PlayerIndex playerIndex) => provider.GetGamepadLeftTrigger(playerIndex);
    public float GetGamepadRightTrigger(PlayerIndex playerIndex) => provider.GetGamepadRightTrigger(playerIndex);
    public float GetGamepadAxis(PlayerIndex playerIndex, GamepadAxis axis) => provider.GetGamepadAxis(playerIndex, axis);

    // ===== Utils inspired from Godot Input system =====

    /// <summary>
    /// Returns the combined value of two actions for an axis
    /// </summary>
    /// <param name="negativeAction">Action that produces a negative value.</param>
    /// <param name="positiveAction">Action that produces a positive value.</param>
    /// <returns>A value between -1 and 1.</returns>
    public float GetAxis(string negativeAction, string positiveAction)
    {
        float negative = _actionMap.GetAction(negativeAction)?.GetValue() ?? 0f;
        float positive = _actionMap.GetAction(positiveAction)?.GetValue() ?? 0f;
        return MathHelper.Clamp(positive - negative, -1f, 1f);
    }

    /// <summary>
    /// Returns a normalized Vector2 from 4 directions.
    /// </summary>
    /// <param name="negativeX">Action for left value.</param>
    /// <param name="positiveX">Action for right value.</param>
    /// <param name="negativeY">Action for up value.</param>
    /// <param name="positiveY">Action for down value.</param>
    /// <returns>Vector2 normalizado (máxima longitud 1).</returns>
    public Vector2 GetVector(string negativeX, string positiveX, string negativeY, string positiveY)
    {
        float x = GetAxis(negativeX, positiveX);
        float y = GetAxis(negativeY, positiveY);
        Vector2 result = new(x, y);
        if (result.LengthSquared() > 1f)
            result.Normalize();

        return result;
    }

    /// <summary>
    /// Returns the combined value of two actions for an axis
    /// </summary>
    /// <param name="negativeAction">Action that produces a negative value.</param>
    /// <param name="positiveAction">Action that produces a positive value.</param>
    /// <returns>A value between -1 and 1.</returns>
    public static float GetAxis(InputAction negativeAction, InputAction positiveAction)
    {
        Debug.Assert(negativeAction != null);
        Debug.Assert(positiveAction != null);

        float negative = negativeAction.GetValue();
        float positive = positiveAction.GetValue();
        return MathHelper.Clamp(positive - negative, -1f, 1f);
    }

    /// <summary>
    /// Returns a normalized Vector2 from 4 directions.
    /// </summary>
    /// <param name="negativeX">Action for left value.</param>
    /// <param name="positiveX">Action for right value.</param>
    /// <param name="negativeY">Action for up value.</param>
    /// <param name="positiveY">Action for down value.</param>
    /// <returns>Vector2 normalizado (máxima longitud 1).</returns>
    public static Vector2 GetVector(InputAction negativeX, InputAction positiveX, InputAction negativeY, InputAction positiveY)
    {
        float x = GetAxis(negativeX, positiveX);
        float y = GetAxis(negativeY, positiveY);
        Vector2 result = new(x, y);
        if (result.LengthSquared() > 1f)
            result.Normalize();

        return result;
    }

    /// <summary>
    /// Returns true if the specified action is pressed
    /// </summary>
    public bool IsActionPressed(string actionName) => _actionMap.GetAction(actionName)?.IsPressed ?? false;

    /// <summary>
    /// Returns true if the specified action is pressed on this frame
    /// </summary>
    public bool WasActionJustPressed(string actionName) => _actionMap.GetAction(actionName)?.WasPressedThisFrame ?? false;

    /// <summary>
    /// Returns true if the specified action is released on this frame
    /// </summary>
    public bool WasActionJustReleased(string actionName) => _actionMap.GetAction(actionName)?.WasReleasedThisFrame ?? false;


    /// <summary>
    /// Returns the value of the specified actions
    /// </summary>
    /// <param name="actionName"></param>
    /// <returns>A value in the range [0,1]</returns>
    public float GetActionValue(string actionName) => _actionMap.GetAction(actionName)?.GetValue() ?? 0f;
}