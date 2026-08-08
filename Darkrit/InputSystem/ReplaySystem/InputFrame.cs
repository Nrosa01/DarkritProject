namespace Darkrit.InputSystem.ReplaySystem;

public readonly struct InputFrame
{
    public readonly KeyboardFrame Keyboard { get; init; }
    public readonly MouseFrame Mouse { get; init; }
    public readonly GamePadFrame[] GamePads { get; init; }
}
