using System;
using System.Collections.Generic;
using System.Text;
using Darkrit.Input.Providers;

namespace Darkrit.Input;

public interface IInputBinding
{
    internal IInputProvider provider { set; }

    public bool Pressed();
    public bool Released() => !Pressed();
    public bool PressedThisFrame();
    public bool ReleasedThisFrame();
    float GetValue();
}
