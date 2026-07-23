using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.Input.Bindings
{
    public class GamepadBinding(Buttons button, MonoGameLibrary.Input.Input input) : IInputBinding
    {
        public bool Pressed() => input.GamePads[0].IsButtonDown(button);

        public bool PressedThisFrame() => input.GamePads[0].WasButtonJustPressed(button);

        public bool ReleasedThisFrame() => input.GamePads[0].WasButtonJustReleased(button);
    }
}
