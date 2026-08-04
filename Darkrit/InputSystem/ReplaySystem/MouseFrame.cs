using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace Darkrit.InputSystem.ReplaySystem
{
    internal readonly struct MouseFrame
    {
        public readonly Point Position { get; init; }
        public readonly int Wheel { get; init; }

        public readonly bool Left { get; init; }
        public readonly bool Middle { get; init; }
        public readonly bool Right { get; init; }
        public readonly bool X1 { get; init; }
        public readonly bool X2 { get; init; }
    }
}
