// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.InputSystem.Providers;

namespace Darkrit.InputSystem;

public interface IInputBinding
{
    internal IInputProvider provider { set; }

    bool Pressed();
    bool Released() => !Pressed();
    bool PressedThisFrame();
    bool ReleasedThisFrame();
    float GetValue();
}
