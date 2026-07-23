using MonoGameLibrary.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.Input.Bindings
{
    public class MouseBinding(MouseButton button, MonoGameLibrary.Input.Input input) : IInputBinding
    {
        public bool Pressed() => input.Mouse.IsButtonDown(button);

        public bool PressedThisFrame() => input.Mouse.WasButtonJustPressed(button);

        public bool ReleasedThisFrame() => input.Mouse.WasButtonJustReleased(button);
    }
}
