// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.InputSystem.ReplaySystem;

namespace Darkrit.InputSystem.Providers;

/// <summary>
/// Input provider that can be serialized
/// </summary>
public interface ISerializableInputProvider : IInputProvider
{
    InputFrame CaptureFrame();
}
