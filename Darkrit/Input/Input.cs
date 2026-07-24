using System.Diagnostics;
using Darkrit.Input.Bindings;
using Darkrit.Input.Providers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Darkrit.Input;

namespace Darkrit.Input;

/// <summary>
/// Main input system. Offers a unified API to check keyboard, mouse
/// and gamepad. 
/// </summary>
/// <remarks>
/// Creates a new Input instance that uses a certain provider
/// </remarks>
/// <param name="provider">Input provider to use.</param>
public class Input(IInputProvider provider)
{
    private IInputProvider _provider = provider;
    private InputMap _actionMap = new();

    /// <summary>
    /// Action map that maps string names to bindings
    /// </summary>
    public InputMap ActionMap => _actionMap;

    /// <summary>
    /// Current hardware provider
    /// </summary>
    public IInputProvider Provider => _provider;

    /// <summary>
    /// Creates a new instance using the physics input provider by default
    /// </summary>
    public Input() : this(new PhysicalInputProvider()) { }

    public InputAction CreateAction(string actionName) => _actionMap.AddAction(actionName, _provider);

    /// <summary>
    /// Changes the input provider in runtime.
    /// This is mainly thought for using a ReplayInputProvider
    /// </summary>
    public void SetProvider(IInputProvider newProvider) => _provider = newProvider;

    /// <summary>
    /// Updates all input states.
    /// Must be called once per frame.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        _provider.Update(gameTime);
    }

    // ===== Direct access methods for when you're lazy or prototyping =====

    // TODO: It's a chore, but I should add stupid xml doc for these helpers

    // Keyboard
    public bool IsKeyDown(Keys key) => _provider.IsKeyDown(key);
    public bool IsKeyUp(Keys key) => _provider.IsKeyUp(key);
    public bool WasKeyJustPressed(Keys key) => _provider.WasKeyJustPressed(key);
    public bool WasKeyJustReleased(Keys key) => _provider.WasKeyJustReleased(key);
    public Keys[] GetPressedKeys() => _provider.GetPressedKeys();

    // Mouse
    public Point GetMousePosition() => _provider.GetMousePosition();
    public int GetMouseScrollWheelValue() => _provider.GetMouseScrollWheelValue();
    public bool IsMouseButtonDown(MouseButton button) => _provider.IsMouseButtonDown(button);
    public bool IsMouseButtonUp(MouseButton button) => _provider.IsMouseButtonUp(button);
    public bool WasMouseButtonJustPressed(MouseButton button) => _provider.WasMouseButtonJustPressed(button);
    public bool WasMouseButtonJustReleased(MouseButton button) => _provider.WasMouseButtonJustReleased(button);
    public Point GetMousePositionDelta() => _provider.GetMousePositionDelta();

    // Gamepad
    public bool IsGamepadConnected(PlayerIndex playerIndex) => _provider.IsGamepadConnected(playerIndex);
    public bool IsGamepadButtonDown(PlayerIndex playerIndex, Buttons button) => _provider.IsGamepadButtonDown(playerIndex, button);
    public bool IsGamepadButtonUp(PlayerIndex playerIndex, Buttons button) => _provider.IsGamepadButtonUp(playerIndex, button);
    public bool WasGamepadButtonJustPressed(PlayerIndex playerIndex, Buttons button) => _provider.WasGamepadButtonJustPressed(playerIndex, button);
    public bool WasGamepadButtonJustReleased(PlayerIndex playerIndex, Buttons button) => _provider.WasGamepadButtonJustReleased(playerIndex, button);
    public Vector2 GetGamepadLeftStick(PlayerIndex playerIndex) => _provider.GetGamepadLeftStick(playerIndex);
    public Vector2 GetGamepadRightStick(PlayerIndex playerIndex) => _provider.GetGamepadRightStick(playerIndex);
    public float GetGamepadLeftTrigger(PlayerIndex playerIndex) => _provider.GetGamepadLeftTrigger(playerIndex);
    public float GetGamepadRightTrigger(PlayerIndex playerIndex) => _provider.GetGamepadRightTrigger(playerIndex);
    public float GetGamepadAxis(PlayerIndex playerIndex, GamepadAxis axis) => _provider.GetGamepadAxis(playerIndex, axis);

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