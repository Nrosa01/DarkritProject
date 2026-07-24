using Darkrit.InputSystem.Providers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Darkrit.InputSystem.Bindings
{
    /// <summary>
    /// Gamepad Axis enum representation
    /// </summary>
    public enum GamepadAxis
    {
        LeftStickX,
        LeftStickY,
        RightStickX,
        RightStickY,
        LeftTrigger,
        RightTrigger
    }

    /// <summary>
    /// Bindings for the analogic axes of the gempad (sticks and trigger)
    /// </summary>
    /// <param name="playerIndex">Player index of the gamepad.</param>
    /// <param name="axis">Gamepad axis to read.</param>
    /// <param name="provider">Input provider to use (will probably be removed in the future).</param>
    /// <param name="deadZone">Deadzone.</param>
    internal class GamepadAxisBinding(
        PlayerIndex playerIndex,
        GamepadAxis axis,
        float deadZone = 0.2f) : IInputBinding
    {
        IInputProvider provider;

        IInputProvider IInputBinding.provider { set => provider = value; }

        // Simplified constructor for when there is only one player
        public GamepadAxisBinding(GamepadAxis axis, float deadZone = 0.2f)
            : this(PlayerIndex.One, axis, deadZone) { }

        /// <summary>
        /// Returns true if the axis value is above the deadzone
        /// </summary>
        public bool Pressed() => MathF.Abs(GetValue()) > deadZone;

        /// <summary>
        /// Whether the Stick or Trigger was pressed just this frame
        /// </summary>
        public bool PressedThisFrame()
        {
            // Provider interface currently doesn't support this, it could but it doesn't make sense
            // at least for thumbsticks, it might make sense for triggers but meh, I don't need it now
            return false;
        }

        public bool ReleasedThisFrame()
        {
            // Provider interface currently doesn't support this, it could but it doesn't make sense
            // at least for thumbsticks, it might make sense for triggers but meh, I don't need it now
            return false;
        }

        /// <summary>
        /// Returns the analogic value for the axis. Ranges from [-1,1] for sticks and [0,1] for triggers
        /// </summary>
        public float GetValue()
        {
            float rawValue = provider.GetGamepadAxis(playerIndex, axis);

            if (MathF.Abs(rawValue) < deadZone)
                return 0f;

            // Normalize range to [0,1] after deadzone
            return MathF.Sign(rawValue) * ((MathF.Abs(rawValue) - deadZone) / (1f - deadZone));
        }
    }
}