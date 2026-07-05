using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;

namespace MonoGameLibrary.Content;

public interface IHotReloadableAsset
{
    public string AssetName { get; }

    public void Reload();
}

public static class ContentManagerExtensions
{
    internal static FileSystemWatcher assetsWatcher = null;
    internal static Process? _hotReloadProcess;

    private static void KillHotReload()
    {
        try
        {
            if (_hotReloadProcess is { HasExited: false })
            {
                _hotReloadProcess.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // ignore
        }
    }

    internal static Dictionary<string, List<IHotReloadableAsset>> watchedAssets = [];
    internal static HashSet<string> assetsToReload = [];
    internal static Lock assetsListLock = new();
    internal static void AddAssetToWatch(IHotReloadableAsset asset)
    {
        if(watchedAssets.TryGetValue(asset.AssetName, out var list))
            list.Add(asset);
        else
            watchedAssets.Add(asset.AssetName, [asset]);
    }

    internal static void StartHotReloadServer()
    {
        if (_hotReloadProcess is { HasExited: false })
            return;

        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
        var name = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);

        _hotReloadProcess = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"msbuild {name}.csproj -t:StartHotReload --tl:off",
            WorkingDirectory = projectRoot,
            UseShellExecute = true
        });
    }

    [Conditional("DEBUG")]
    public static void StartContentWatcherTask()
    {
        StartHotReloadServer();

        new Thread(() =>
        {
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Content\\");
            Debug.WriteLine(outputPath);
            assetsWatcher = new(Path.Combine(Directory.GetCurrentDirectory(), "Content\\"))
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            assetsWatcher.Changed += OnAssetChanged;
            assetsWatcher.Created += OnAssetChanged;

            Thread.Sleep(Timeout.Infinite);
        })
        {
            IsBackground = true
        }.Start();

        // when this program exits, make sure to emit a kill signal to the watcher process
        AppDomain.CurrentDomain.ProcessExit += (_, __) =>
        {
            try
            {
                KillHotReload();
                assetsWatcher?.Dispose();
            }
            catch
            {
                /* ignore */
            }
        };
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            try
            {
                KillHotReload();
                assetsWatcher?.Dispose();
            }
            catch
            {
                /* ignore */
            }
        };
    }

    private static void OnAssetChanged(object sender, FileSystemEventArgs e)
    {
        lock(assetsListLock) {
            if(assetsToReload.Add(Path.ChangeExtension(e.Name, null)))
                Debug.WriteLine($"Path is {e.Name}, Full Path is {e.FullPath}, Relative Path is {Path.GetRelativePath(Core.Content.RootDirectory, Core.Content.RootDirectory)}");
        }
    }

    extension(ContentManager content)
    {
        // Only here so it can be called from a ContentManager, as it makes sense for a
        // ContentManager to have this function even if it doesn't access its fields
        /// <summary>
        /// Call Reload on all registered assets for hot reloading
        /// This function should be called once every frame
        /// </summary>
        public void ReloadChangedAssets()
        {
            lock (assetsListLock)
            {
                foreach (var assetName in assetsToReload)
                {
                    if(watchedAssets.TryGetValue(assetName, out var ireloadablelist))
                    {
                        foreach (var reloadable in ireloadablelist)
                            reloadable.Reload();
                    }
                }

                assetsToReload.Clear();
            }
        }

        public WatchedAsset<T> Watch<T>(string assetName)
        {
            var asset = content.Load<T>(assetName);

            // FileSystemWatcher in windows generates uses \ as path separator.
            // Given sometimes I might write \ or /, I have to normalize the paths
            // or else the hot reloading won't work
            var normalized = assetName
                                .Replace('\\', Path.DirectorySeparatorChar)
                                .Replace('/', Path.DirectorySeparatorChar);

            var newAsset = new WatchedAsset<T>
            {
                AssetName = normalized,
                Asset = asset,
                UpdatedAt = DateTimeOffset.Now,
                Owner = content
            };

            AddAssetToWatch(newAsset);

            return newAsset;
        }

        /// <summary>  
        /// Load an Effect into the <see cref="Material"/> wrapper class  
        /// </summary>  
        /// <param name="manager"></param>  
        /// <param name="assetName"></param>  
        /// <returns></returns>  
        public Material WatchMaterial(string assetName)
        {
            return new Material(content.Watch<Effect>(assetName));
        }


        public bool TryRefresh<T>(WatchedAsset<T> watchedAsset, out T oldAsset)
        {
            oldAsset = default;

            // ensure the ContentManager is the same one that loaded the asset
            if (content != watchedAsset.Owner)
            {
                throw new ArgumentException($"Used the wrong ContentManager to refresh {watchedAsset.AssetName}");
            }

            // get the same path that the ContentManager would use to load the asset
            var path = Path.Combine(content.RootDirectory, watchedAsset.AssetName) + ".xnb";

            // ask the operating system when the file was last written.
            var lastWriteTime = File.GetLastWriteTime(path);

            //  when the file's write time is less recent than the asset's latest read time, 
            //  then the asset does not need to be reloaded.
            if (lastWriteTime <= watchedAsset.UpdatedAt)
            {
                return false;
            }

            // wait for the file to not be locked.
            if (IsFileLocked(path)) return false;

            // clear the old asset to avoid leaking
            content.UnloadAsset(watchedAsset.AssetName);

            // return the old asset
            oldAsset = watchedAsset.Asset;

            // load the new asset and update the latest read time
            watchedAsset.Asset = content.Load<T>(watchedAsset.AssetName);
            watchedAsset.UpdatedAt = lastWriteTime;

            return true;
        }
    }

    private static bool IsFileLocked(string path)
    {
        try
        {
            using FileStream _ = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            // File is not locked
            return false;
        }
        catch (IOException)
        {
            // File is locked or inaccessible
            return true;
        }
    }

}
