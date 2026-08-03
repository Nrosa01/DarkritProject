using System;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.InputSystem.ReplaySystem
{
    internal struct InputFrame
    {
        public KeyboardFrame Keyboard;
        public MouseFrame Mouse;
        public GamePadFrame[] GamePads;
    }
}
