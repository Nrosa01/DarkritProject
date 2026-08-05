using System;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.InputSystem.ReplaySystem
{
    internal readonly struct InputFrame
    {
        public readonly bool Enabled { get; init; }
        public readonly KeyboardFrame Keyboard { get; init; }
        public readonly MouseFrame Mouse { get; init; }
        public readonly GamePadFrame[] GamePads { get; init; }
    }
}
