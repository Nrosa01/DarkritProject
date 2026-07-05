/// <summary>
/// Entry point for the Content Builder project, 
/// which when executed will build content according to the "Content Collection Strategy" defined in the Builder class.
/// </summary>
/// <remarks>
/// Make sure to validate the directory paths in the "ContentBuilderParams" for your specific project.
/// For more details regarding the Content Builder, see the MonoGame documentation: <tbc.>
/// </remarks>

using System.Diagnostics;
using Content.Builder;
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;

var contentCollectionArgs = new ContentBuilderParams()
{
    Mode = ContentBuilderMode.Builder,
    WorkingDirectory = $"{AppContext.BaseDirectory}../../", // path to where your content folder can be located
    SourceDirectory = "Assets", // Not actually needed as this is the default, but added for reference
    Platform = TargetPlatform.DesktopGL
};
var builder = new DarkBuilder();

if (args is not null && args.Length > 0)
{
    Console.WriteLine("Running with args");
    foreach (var arg in args)
    {
        Console.WriteLine($"  {arg}");
    }

    if (args.Contains("--hotreload"))
    {
        Console.WriteLine("With hot reload");
        var contentArgs = ContentBuilderParams.Parse(args[..^1]);
        builder.Run(contentArgs);
        HotReloadServer.Create(contentArgs);
    }
    else
    {
        Console.WriteLine("Without hot reload");
        builder.Run(args);

        //var newArgs = args.Append("--hotreload");

        //Process.Start(new ProcessStartInfo
        //{
        //    FileName = Environment.ProcessPath!,
        //    Arguments = string.Join(" ", newArgs),
        //    UseShellExecute = true
        //});
    }
}
else
{
    Console.WriteLine("Running content collection args");
    builder.Run(contentCollectionArgs);
}

return builder.FailedToBuild > 0 ? -1 : 0;

public class DarkBuilder : ContentBuilder
{
    public override IContentCollection GetContentCollection()
    {
        var contentCollection = new ContentCollection();
        contentCollection.Include<WildcardRule>("*");
        contentCollection.IncludeCopy<WildcardRule>("*.xml");
        contentCollection.IncludeCopy<WildcardRule>("*.ttf");
        contentCollection.IncludeCopy<WildcardRule>("*.fnt");
        contentCollection.Exclude<WildcardRule>("*.txt");
        contentCollection.Exclude<WildcardRule>("*.fxh");
        contentCollection.Exclude<WildcardRule>("fmod/*");
        contentCollection.IncludeCopy<WildcardRule>(
            "fmod/Build/Desktop/*.bank",
            outputPath: filePath => $"fmod/Desktop/{Path.GetFileName(filePath)}");

        return contentCollection;
    }
}