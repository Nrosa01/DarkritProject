// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.CompilerServices;

namespace Darkrit.Utilities;

/// <summary>
/// Profiler utility class to make simple time measurements
/// </summary>
public class Profiler
{
    /// <summary>
    /// When it's debug, measures a function execution time. In release the
    /// function gets inlined
    /// </summary>
    /// <param name="function">The function to profile</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Profile(Action function)
    {
#if DEBUG
        var timer = Stopwatch.StartNew();

        function();

        timer.Stop();

        Log.Debug($"Function: {function.Method.DeclaringType?.FullName}.{function.Method.Name}");
        Log.Debug($"  Time taken: {timer.Elapsed:mm\\:ss\\.fff}");
#else
        function();
#endif
    }
}
