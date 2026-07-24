using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Darkrit.Input;
using System.Collections.Generic;

namespace Darkrit.Input.Recording;

public class FrameInputSnapshot
{
    public float Time { get; set; }
    public HashSet<Keys> KeysDown { get; set; }
    public Point MousePosition { get; set; }
    public HashSet<MouseButton> MouseButtonsDown { get; set; }
    public int ScrollWheelValue { get; set; }
    public List<GamepadSnapshot> Gamepads { get; set; }
}

public class GamepadSnapshot
{
    public PlayerIndex PlayerIndex { get; set; }
    public HashSet<Buttons> ButtonsDown { get; set; }
    public Vector2 LeftStick { get; set; }
    public Vector2 RightStick { get; set; }
    public float LeftTrigger { get; set; }
    public float RightTrigger { get; set; }
}