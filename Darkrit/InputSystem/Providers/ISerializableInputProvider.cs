using Darkrit.InputSystem.ReplaySystem;

namespace Darkrit.InputSystem.Providers;

public interface ISerializableInputProvider : IInputProvider
{
    InputFrame CaptureFrame();
}
