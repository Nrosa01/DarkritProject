using System.Drawing;

namespace Darkrit.InputSystem.ReplaySystem;

public readonly struct MouseFrame
{
    public readonly Point Position { get; init; }
    public readonly int Wheel { get; init; }

    public readonly bool Left { get; init; }
    public readonly bool Middle { get; init; }
    public readonly bool Right { get; init; }
    public readonly bool X1 { get; init; }
    public readonly bool X2 { get; init; }
}
