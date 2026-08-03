using System;
using System.Collections.Generic;
using System.Text;
using Darkrit.InputSystem.ReplaySystem;

namespace Darkrit.InputSystem.Providers
{
    internal interface ISerializableInputProvider : IInputProvider
    {
        InputFrame CaptureFrame();
    }
}
