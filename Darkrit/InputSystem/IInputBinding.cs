using System;
using System.Collections.Generic;
using System.Text;
using Darkrit.InputSystem.Providers;

namespace Darkrit.InputSystem;

public interface IInputBinding
{
    internal IInputProvider provider { set; }

    public bool Pressed();
    public bool Released() => !Pressed();
    public bool PressedThisFrame();
    public bool ReleasedThisFrame();
    float GetValue();
}
