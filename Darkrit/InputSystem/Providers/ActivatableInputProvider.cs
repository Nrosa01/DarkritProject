using Darkrit.InputSystem.Bindings;
using Darkrit.InputSystem.ReplaySystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Darkrit.InputSystem.Providers;

public class ActivatableInputProvider(ISerializableInputProvider provider) : ISerializableInputProvider
{
    private ISerializableInputProvider _currentProvider = provider;
    private readonly ISerializableInputProvider _mainProvider = provider;
    private readonly NullInputProvider _nullInput = new();

    public bool Enabled
    {
        get;
        set
        {
            field = value;
            if (value)
                _currentProvider = _mainProvider;
            else
                _currentProvider = _nullInput;
        }
    }

    /// <summary>
    /// Current hardware provider
    /// </summary>
    public IInputProvider Provider => _currentProvider;

    /// <summary>
    /// Creates a new instance using the physics input provider by default
    /// </summary>
    public ActivatableInputProvider() : this(new PhysicalInputProvider()) { }


    bool _enabledLastFrame;
    /// <summary>
    /// Updates all input states.
    /// Must be called once per frame.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        _currentProvider.Update(gameTime);

        if (Enabled != _enabledLastFrame)
            _nullInput.LastRecordedMousePosition = _mainProvider.GetMousePosition();

        _enabledLastFrame = Enabled;
    }

    // ===== Direct access methods for when you're lazy or prototyping =====

    // TODO: It's a chore, but I should add stupid xml doc for these helpers

    // Keyboard
    public bool IsKeyDown(Keys key) => _currentProvider.IsKeyDown(key);
    public bool IsKeyUp(Keys key) => _currentProvider.IsKeyUp(key);
    public bool WasKeyJustPressed(Keys key) => _currentProvider.WasKeyJustPressed(key);
    public bool WasKeyJustReleased(Keys key) => _currentProvider.WasKeyJustReleased(key);
    public Keys[] GetPressedKeys() => _currentProvider.GetPressedKeys();

    // Mouse
    public Point GetMousePosition() => _currentProvider.GetMousePosition();
    public int GetMouseScrollWheelValue() => _currentProvider.GetMouseScrollWheelValue();
    public bool IsMouseButtonDown(MouseButton button) => _currentProvider.IsMouseButtonDown(button);
    public bool IsMouseButtonUp(MouseButton button) => _currentProvider.IsMouseButtonUp(button);
    public bool WasMouseButtonJustPressed(MouseButton button) => _currentProvider.WasMouseButtonJustPressed(button);
    public bool WasMouseButtonJustReleased(MouseButton button) => _currentProvider.WasMouseButtonJustReleased(button);
    public Point GetMousePositionDelta() => _currentProvider.GetMousePositionDelta();

    // Gamepad
    public bool IsGamepadConnected(PlayerIndex playerIndex) => _currentProvider.IsGamepadConnected(playerIndex);
    public bool IsGamepadButtonDown(PlayerIndex playerIndex, Buttons button) => _currentProvider.IsGamepadButtonDown(playerIndex, button);
    public bool IsGamepadButtonUp(PlayerIndex playerIndex, Buttons button) => _currentProvider.IsGamepadButtonUp(playerIndex, button);
    public bool WasGamepadButtonJustPressed(PlayerIndex playerIndex, Buttons button) => _currentProvider.WasGamepadButtonJustPressed(playerIndex, button);
    public bool WasGamepadButtonJustReleased(PlayerIndex playerIndex, Buttons button) => _currentProvider.WasGamepadButtonJustReleased(playerIndex, button);
    public Vector2 GetGamepadLeftStick(PlayerIndex playerIndex) => _currentProvider.GetGamepadLeftStick(playerIndex);
    public Vector2 GetGamepadRightStick(PlayerIndex playerIndex) => _currentProvider.GetGamepadRightStick(playerIndex);
    public float GetGamepadLeftTrigger(PlayerIndex playerIndex) => _currentProvider.GetGamepadLeftTrigger(playerIndex);
    public float GetGamepadRightTrigger(PlayerIndex playerIndex) => _currentProvider.GetGamepadRightTrigger(playerIndex);
    public float GetGamepadAxis(PlayerIndex playerIndex, GamepadAxis axis) => _currentProvider.GetGamepadAxis(playerIndex, axis);

    public InputFrame CaptureFrame() => _currentProvider.CaptureFrame();
}
