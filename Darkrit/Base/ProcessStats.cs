// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;

namespace Darkrit.Base;

/// <summary>
/// Utility class to track Process State with discrete updates
/// </summary>
/// <param name="Process"></param>
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
