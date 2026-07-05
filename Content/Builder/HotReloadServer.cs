using MonoGame.Framework.Content.Pipeline.Builder;

namespace Content.Builder
{
    internal class HotReloadServer
    {
        static Timer? _timer;

        static void ScheduleRebuild(ContentBuilderParams contentArgs)
        {
            _timer?.Dispose();

            _timer = new Timer(_ =>
            {
                Rebuild(contentArgs);
            }, null, 300, Timeout.Infinite);
        }


        static int rebuildCount = 0;
        static void Rebuild(ContentBuilderParams contentArgs)
        {
            Console.WriteLine($"Rebuilding {++rebuildCount}");
            new DarkBuilder().Run(contentArgs);
        }

        public static void Create(ContentBuilderParams contentArgs)
        {
            var assetsSource = contentArgs.RootedSourceDirectory;
            var output = contentArgs.RootedOutputDirectory;

            Console.WriteLine($"Watching {assetsSource}");
            Console.WriteLine($"Outputing to {output}");

            FileSystemWatcher watcher = new(assetsSource)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
            };

            watcher.Changed += (_, _) => ScheduleRebuild(contentArgs);

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                watcher?.Dispose();
            };

            AppDomain.CurrentDomain.ProcessExit += (_, __) =>
            {
                watcher?.Dispose();
            };

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
