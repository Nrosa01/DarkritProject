using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.Input.Bindings
{
    public class KeyboardBinding(Keys key, MonoGameLibrary.Input.Input inputHelper) : IInputBinding
    {
        public bool Pressed() => inputHelper.Keyboard.IsKeyDown(key);

        public bool PressedThisFrame() => inputHelper.Keyboard.WasKeyJustPressed(key);

        public bool ReleasedThisFrame() => inputHelper.Keyboard.WasKeyJustReleased(key);
    }
}
