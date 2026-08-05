using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Darkrit.Base
{
    internal class ProcessStats(Process Process)
    {
#if EDITOR_BUILD
        readonly double currentProcessUpdateInterval = 1.5f;
        double currentProcessUpdateIntervalTimer = 0.0f;
#endif
        public Process Process { get; } = Process;

        [Conditional("EDITOR_BUILD")]
        public void Update(double delta)
        {
#if EDITOR_BUILD
            currentProcessUpdateIntervalTimer += delta;
            if (currentProcessUpdateIntervalTimer > currentProcessUpdateInterval)
            {
                Process.Refresh();
                currentProcessUpdateIntervalTimer -= currentProcessUpdateInterval;
            }
#endif
        }
    }
}
