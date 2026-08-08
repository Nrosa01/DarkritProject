using System.Numerics;

namespace Darkrit.InputSystem.ReplaySystem;

public readonly struct GamePadFrame
{
    public readonly bool Connected { get; init; }

    public readonly GamePadButtons Buttons { get; init; }

    public readonly Vector2 LeftStick { get; init; }
    public readonly Vector2 RightStick { get; init; }

    public readonly float LeftTrigger { get; init; }
    public readonly float RightTrigger { get; init; }
}
