using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Darkrit.Editor;

/// <summary>
/// Class that holds all of the EditorData related functionality
/// </summary>
public static class EditorData
{
    public static readonly string UserDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DarkritEditor");

    public static string ProjectEditorDataDirectory => Path.Combine(EditorProjectPath.Root, "..", ".darkrit");

    /// <summary>
    /// Exports all the DarkritEditor UserDirectory data to the SolutionRoot/.darkrit so it's easier to transfer between devices
    /// </summary>
    /// <param name="name"></param>
    public static void Export(string name)
    {
        Directory.CreateDirectory(ProjectEditorDataDirectory);

        string path = Path.Combine(ProjectEditorDataDirectory, $"{name}.zip");

        if (File.Exists(path))
            File.Delete(path);

        ZipFile.CreateFromDirectory(UserDirectory, path, CompressionLevel.Fastest, false);
    }

    /// <summary>
    /// Gets all zip files in <see cref="ProjectEditorDataDirectory"/>.
    /// Right now it doesn't check that they're EditorData packs, just zips
    /// </summary>
    /// <returns></returns>
    public static string[] GetAvailablePacks()
    {
        if (!Directory.Exists(ProjectEditorDataDirectory))
            return [];

        return [.. Directory
            .GetFiles(ProjectEditorDataDirectory, "*.zip")
            .Select(Path.GetFileNameWithoutExtension)];
    }


    /// <summary>
    /// Imports an editor data pack. It just puts into the correct folder all the data
    /// But layouts and other stuff aren't aware of this, an Editor Reboot is currentely needed
    /// until a filesystem watcher is implemented to handle that
    /// </summary>
    /// <param name="name"></param>
    public static void Import(string name)
    {
        string path = Path.Combine(ProjectEditorDataDirectory, $"{name}.zip");

        if (!File.Exists(path))
            return;

        Directory.CreateDirectory(UserDirectory);

        ZipFile.ExtractToDirectory(path, UserDirectory, true);
    }
}
