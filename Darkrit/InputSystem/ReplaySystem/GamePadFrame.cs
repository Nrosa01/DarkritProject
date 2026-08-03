using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Darkrit.InputSystem.ReplaySystem
{
    internal struct GamePadFrame
    {
        public bool Connected;

        public GamePadButtons Buttons;

        public Vector2 LeftStick;
        public Vector2 RightStick;

        public float LeftTrigger;
        public float RightTrigger;
    }
}
