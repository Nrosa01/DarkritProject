using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Darkrit.InputSystem.ReplaySystem
{
    internal readonly struct GamePadFrame
    {
        public readonly bool Connected { get; init; }

        public readonly GamePadButtons Buttons { get; init; }

        public readonly Vector2 LeftStick { get; init; }
        public readonly Vector2 RightStick { get; init; }

        public readonly float LeftTrigger { get; init; }
        public readonly float RightTrigger { get; init; }
    }
}
