using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Darkrit.Input
{
    public class InputAction(List<IInputBinding> bindings) : IInputBinding
    {
        public bool Pressed() => bindings.Any(x => x.Pressed());

        public bool PressedThisFrame() => bindings.Any(x => x.PressedThisFrame());

        public bool ReleasedThisFrame() => bindings.Any(x => x.ReleasedThisFrame());
    }
}
