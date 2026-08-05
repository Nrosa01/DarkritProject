using Darkrit.DevTools.Logger;
using System;
using System.Diagnostics;

namespace Darkrit.Utilities
{
    public class Profiler
    {
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

        public static void Profile(string name, Action function)
        {
#if DEBUG
            var timer = Stopwatch.StartNew();

            function();

            timer.Stop();

            Log.Debug($"{name}: {timer.Elapsed:mm\\:ss\\.fff}");
#else
            function();
#endif
        }
    }
}
