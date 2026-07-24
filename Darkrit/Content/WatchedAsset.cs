using Microsoft.Xna.Framework.Content;
using System;

namespace Darkrit.Content;


public class WatchedAsset<T> : IHotReloadableAsset, IDisposable
{
    public event Action<T> AssetChanged;

    public void AddAssetChangedListener(Action<T> function) => AssetChanged += function;

    public void RemoveAssetChangedListener(Action<T> function) => AssetChanged -= function;

    /// <summary>
    /// The latest version of the asset.
    /// </summary>
    public T Asset { get; set; }

    /// <summary>
    /// The last time the <see cref="Asset"/> was loaded into memory.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// The name of the <see cref="Asset"/>. This is the name used to load the asset from disk. 
    /// </summary>
    public string AssetName { get; init; }

    /// <summary>  
    /// The <see cref="ContentManager"/> instance that loaded the asset.  
    /// </summary> 
    public ContentManager Owner { get; init; }

    public void Reload()
    {
        if (Owner.TryRefresh(this, out var oldAsset))
            AssetChanged?.Invoke(oldAsset);
    }

    /// <summary>  
    /// Attempts to refresh the asset if it has changed on disk using the registered owner <see cref="ContentManager"/>.  
    /// </summary>
    public bool TryRefresh(out T oldAsset) => Owner.TryRefresh(this, out oldAsset);

    ~WatchedAsset() => Dispose();

    public void Dispose()
    {
        AssetChanged = null;
        GC.SuppressFinalize(this);
    }

    public static implicit operator T(WatchedAsset<T> wachedAsset) => wachedAsset.Asset;
}
