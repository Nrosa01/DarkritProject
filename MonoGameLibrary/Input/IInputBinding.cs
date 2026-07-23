using System;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.Input
{
    public interface IInputBinding
    {
        public bool Pressed();
        public bool Released() => !Pressed();
        public bool PressedThisFrame();
        public bool ReleasedThisFrame();
    }
}
