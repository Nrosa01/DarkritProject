using System;
using System.Collections.Generic;
using System.Text;
using Darkrit.InputSystem.Bindings;
using Darkrit.InputSystem.ReplaySystem;
using Darkrit.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Darkrit.InputSystem.Providers
{
    internal class ReplayInputProvider : IInputProvider
    {
        InputFrame _previousFrame;
        InputFrame _currentFrame;
        int _frame = 0;

        public event Action OnPlaybackFinished;

        IReadOnlyList<InputFrame> _replayFrames = [];

        bool replaying = false;

        public bool IsReplaying => replaying;

        public void StartReplay(IReadOnlyList<InputFrame> replayFrames)
        {
            replaying = true;
            _replayFrames = replayFrames;
            _frame = 0;
        }

        public int TotalFrames => _replayFrames.Count;

        public int CurrentFrame => _frame;

        public void StopReplay()
        {
            replaying = false;
        }

        public void Update(GameTime gameTime)
        {
            if (!replaying || _replayFrames == null || _replayFrames.Count == 0) return;

            if (_frame > _replayFrames.Count - 1)
            {
                StopReplay();
                OnPlaybackFinished?.Invoke();
                return;
            }

            if(_frame > 0)
                _previousFrame = _replayFrames[_frame - 1];
            
            _currentFrame = _replayFrames[_frame];
            
            _frame++;
        }

        // Keyboard
        public bool IsKeyDown(Keys key) => KeyboardFrame.IsPressed(_currentFrame.Keyboard, (int)key);
        public bool IsKeyUp(Keys key) => !KeyboardFrame.IsPressed(_currentFrame.Keyboard, (int)key);
        public bool WasKeyJustPressed(Keys key) => KeyboardFrame.IsPressed(_currentFrame.Keyboard, (int)key) && !KeyboardFrame.IsPressed(_previousFrame.Keyboard, (int)key);
        public bool WasKeyJustReleased(Keys key) => !KeyboardFrame.IsPressed(_currentFrame.Keyboard, (int)key) && KeyboardFrame.IsPressed(_previousFrame.Keyboard, (int)key);
        public Keys[] GetPressedKeys() => throw new NotImplementedException(); // I don't feel like implementing this yet

        // Mouse

        private static Point GetMousePosition(MouseFrame frame) => frame.Position.AsMonoGamePoint();
        public Point GetMousePosition() => GetMousePosition(_currentFrame.Mouse);
        public int GetMouseScrollWheelValue() => _currentFrame.Mouse.Wheel;
        private static bool IsMouseButtonDown(MouseButton button, MouseFrame frame) => button switch
        {
            MouseButton.Left => frame.Left,
            MouseButton.Middle => frame.Middle,
            MouseButton.Right => frame.Right,
            MouseButton.XButton1 => frame.X1,
            MouseButton.XButton2 => frame.X2,
            _ => throw new NotImplementedException(),
        };

        private bool IsMouseButtonUp(MouseButton button, MouseFrame frame) => !IsMouseButtonDown(button, frame);


        public bool IsMouseButtonDown(MouseButton button) => IsMouseButtonDown(button, _currentFrame.Mouse);

        public bool IsMouseButtonUp(MouseButton button) => !IsMouseButtonDown(button);

        public bool WasMouseButtonJustPressed(MouseButton button) => IsMouseButtonDown(button, _currentFrame.Mouse) && IsMouseButtonUp(button, _previousFrame.Mouse);
        public bool WasMouseButtonJustReleased(MouseButton button) => IsMouseButtonUp(button, _currentFrame.Mouse) && IsMouseButtonDown(button, _previousFrame.Mouse);
        public Point GetMousePositionDelta() => GetMousePosition(_currentFrame.Mouse) - GetMousePosition(_previousFrame.Mouse);

        // Gamepad
        public bool IsGamepadConnected(PlayerIndex playerIndex) => _currentFrame.GamePads[(int)playerIndex].Connected;

        public static bool IsGamepadButtonDown(PlayerIndex playerIndex, Buttons button, GamePadFrame[] frame)
        {
            GamePadButtons currentButtons = frame[(int)playerIndex].Buttons;
            if (currentButtons == GamePadButtons.None) return false;

            return (currentButtons & (GamePadButtons)button) == (GamePadButtons)button;
        }

        public bool IsGamepadButtonDown(PlayerIndex playerIndex, Buttons button) => IsGamepadButtonDown(playerIndex, button, _currentFrame.GamePads);

        public static bool IsGamepadButtonUp(PlayerIndex playerIndex, Buttons button, GamePadFrame[] frame)
        {
            GamePadButtons currentButtons = frame[(int)playerIndex].Buttons;
            if (currentButtons == GamePadButtons.None) return false;

            return (currentButtons & (GamePadButtons)button) != (GamePadButtons)button;
        }

        public bool IsGamepadButtonUp(PlayerIndex playerIndex, Buttons button) => IsGamepadButtonUp(playerIndex, button, _currentFrame.GamePads);
        public bool WasGamepadButtonJustPressed(PlayerIndex playerIndex, Buttons button) => IsGamepadButtonDown(playerIndex, button, _currentFrame.GamePads) && IsGamepadButtonUp(playerIndex, button, _previousFrame.GamePads);
        public bool WasGamepadButtonJustReleased(PlayerIndex playerIndex, Buttons button) => IsGamepadButtonUp(playerIndex, button, _currentFrame.GamePads) && IsGamepadButtonDown(playerIndex, button, _previousFrame.GamePads);
        public Vector2 GetGamepadLeftStick(PlayerIndex playerIndex) => _currentFrame.GamePads[(int)playerIndex].LeftStick;
        public Vector2 GetGamepadRightStick(PlayerIndex playerIndex) => _currentFrame.GamePads[(int)playerIndex].RightStick;
        public float GetGamepadLeftTrigger(PlayerIndex playerIndex) => _currentFrame.GamePads[(int)playerIndex].LeftTrigger;
        public float GetGamepadRightTrigger(PlayerIndex playerIndex) => _currentFrame.GamePads[(int)playerIndex].RightTrigger;
        public float GetGamepadAxis(PlayerIndex playerIndex, GamepadAxis axis)
        {
            var gp = _currentFrame.GamePads[(int)playerIndex];
            return axis switch
            {
                GamepadAxis.LeftStickX => gp.LeftStick.X,
                GamepadAxis.LeftStickY => gp.LeftStick.Y,
                GamepadAxis.RightStickX => gp.RightStick.X,
                GamepadAxis.RightStickY => gp.RightStick.Y,
                GamepadAxis.LeftTrigger => gp.LeftTrigger,
                GamepadAxis.RightTrigger => gp.RightTrigger,
                _ => 0f
            };
        }
    }
}
