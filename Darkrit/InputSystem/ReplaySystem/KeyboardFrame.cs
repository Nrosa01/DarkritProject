using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Darkrit.InputSystem.ReplaySystem
{
    internal struct KeyboardFrame
    {
        public ulong B0;
        public ulong B1;
        public ulong B2;
        public ulong B3;

        public void Set(Keys key)
        {
            int value = (int)key;
            int index = value >> 6;
            ulong bit = 1UL << (value & 63);

            switch (index)
            {
                case 0: B0 |= bit; break;
                case 1: B1 |= bit; break;
                case 2: B2 |= bit; break;
                case 3: B3 |= bit; break;
            }
        }

        static bool IsPressed(in KeyboardFrame frame, int key)
        {
            int index = key >> 6;      // /64
            int bit = key & 63;        // %64

            ulong value = index switch
            {
                0 => frame.B0,
                1 => frame.B1,
                2 => frame.B2,
                _ => frame.B3
            };

            return (value & (1UL << bit)) != 0;
        }
    }
}
