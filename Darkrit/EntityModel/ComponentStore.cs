using Darkrit.Base;
using Darkrit.DataStructures;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Darkrit.EntityModel;

public interface IComponentStore
{
    public void InitializePendingComponents();
    public void Update(GameTime gameTime);
    public void FixedUpdate(GameTime gameTime);
    public void Draw(GameTime gameTime);

    public bool TryRemove(Handle<IComponent> handle);
    public bool TryRemove(Handle handle);
}

internal struct ComponentMetadata
{
    internal bool _enabled = false;
    internal bool _initialized = false;

    public readonly bool CanExecute => true;
    //public readonly bool CanExecute => _enabled && _initialized;

    public ComponentMetadata()
    {
    }
}

public class ComponentStore<T>(int initialCapacity) : IComponentStore where T : struct, IComponent
{
    private static readonly bool IsUpdateable = typeof(T).IsDefined(typeof(UpdateableAttribute), inherit: false);

    private static readonly bool IsFixedUpdateable = typeof(T).IsDefined(typeof(FixedUpdateableAttribute), inherit: false);

    private static readonly bool IsDrawable = typeof(T).IsDefined(typeof(DrawableAttribute), inherit: false);

    private readonly HandleMapGrowing<T> _components = new(initialCapacity);
    GrowableArray<ComponentMetadata> _componentMetadata = new(initialCapacity);
    private Stack<Handle<T>> nonInitializedComponents = new();

    public int Count => _components.Count;

    public void InitializePendingComponents()
    {
        while (nonInitializedComponents.TryPop(out Handle<T> handle))
            _components[handle].Start();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<T> Add(T value)
    {
        var handle = _components.Add(value);
        nonInitializedComponents.Push(handle);

        if (handle.Id < _componentMetadata.Count)
            _componentMetadata[handle.Id] = default;
        else
            _componentMetadata.Add(default);

        return handle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get(Handle<T> componentHandle) => ref _components[componentHandle];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Handle<T> componentHandle) => _components.IsValid(componentHandle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRemove(Handle<T> componentHandle) => _components.Remove(componentHandle);

    public void Update(GameTime gameTime)
    {
        if (!IsUpdateable) return;

        foreach (ref var handleItem in _components)
        {
            if (_componentMetadata[handleItem.Handle.Id].CanExecute)
                handleItem.Item.Update(gameTime);
        }
    }

    public void FixedUpdate(GameTime gameTime)
    {
        if (!IsFixedUpdateable) return;

        foreach (ref var handleItem in _components)
        {
            if (_componentMetadata[handleItem.Handle.Id].CanExecute)
                handleItem.Item.FixedUpdate(gameTime);
        }
    }

    public void Draw(GameTime gameTime)
    {
        if (!IsDrawable) return;

        foreach (ref var handleItem in _components)
        {
            if (_componentMetadata[handleItem.Handle.Id].CanExecute)
                handleItem.Item.Draw(gameTime);
        }
    }

    public bool TryRemove(Handle<IComponent> handle)
    {
        return TryRemove(new Handle<T>
        {
            Id = handle.Id,
            Generation = handle.Generation
        });
    }

    public bool TryRemove(Handle handle)
    {
        return TryRemove(new Handle<T>
        {
            Id = handle.Id,
            Generation = handle.Generation
        });
    }
}
