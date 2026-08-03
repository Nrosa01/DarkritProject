using Darkrit.InputSystem.Providers;

namespace Darkrit.InputSystem;

public interface IInputBinding
{
    internal ISerializableInputProvider provider { set; }

    public bool Pressed();
    public bool Released() => !Pressed();
    public bool PressedThisFrame();
    public bool ReleasedThisFrame();
    float GetValue();
}
