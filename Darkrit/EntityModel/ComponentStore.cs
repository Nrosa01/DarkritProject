using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Darkrit.Base;
using Darkrit.DataStructures;
using Microsoft.Xna.Framework;

namespace Darkrit.EntityModel;

public interface IComponentStore
{
    public void InitializePendingComponents();
    public void Update(GameTime gameTime);
    public void FixedUpdate(GameTime gameTime);
    public void Draw(GameTime gameTime);

    public bool TryRemove(Handle<IComponent> handle);
    public bool TryRemove(Handle handle);
    public void EntityActiveInHierarchyChanged(bool status, Handle handle);
    void UpdateComponent(Handle handle, GameTime gameTime);
    public void FixedUpdateComponent(Handle handle, GameTime gameTime);
    void DrawComponent(Handle handle, GameTime gameTime);
    void LateUpdate(GameTime gameTime);
    void LateUpdateComponent(Handle handle, GameTime gameTime);

    bool IsUpdateable { get; }
    bool IsFixedUpdateable { get; }
    bool IsDrawable { get; }

    int Priority { get; }
}

public class ComponentStore<T>(int initialCapacity) : IComponentStore, IEnumerable<T> where T : struct, IComponent, IHandle<T>
{
    private static readonly bool IsUpdateable = typeof(T).IsDefined(typeof(UpdateableAttribute), inherit: false);
    
    private static readonly bool IsLateUpdateable = typeof(T).IsDefined(typeof(LateUpdateableAttribute), inherit: false);

    private static readonly bool IsFixedUpdateable = typeof(T).IsDefined(typeof(FixedUpdateableAttribute), inherit: false);

    private static readonly bool IsDrawable = typeof(T).IsDefined(typeof(DrawableAttribute), inherit: false);
    
    private static readonly bool OverridesPriority = typeof(T).IsDefined(typeof(PriorityAttribute), inherit: false);
    
    public static readonly int Priority = OverridesPriority ? typeof(T).GetCustomAttribute<PriorityAttribute>(inherit: false).Priority : 0;

    /// <inheritdoc/>
    bool IComponentStore.IsUpdateable => IsUpdateable;

    /// <inheritdoc/>
    bool IComponentStore.IsFixedUpdateable => IsFixedUpdateable;

    bool IComponentStore.IsDrawable => IsDrawable;

    private readonly HandleMapGrowing<T> _components = new(initialCapacity);
    private Stack<Handle<T>> nonInitializedComponents = new();

    public int Count => _components.Count;

    int IComponentStore.Priority => Priority;

    public void InitializePendingComponents()
    {
        while (nonInitializedComponents.TryPop(out Handle<T> handle))
            _components[handle].OnAdd();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Add(T value)
    {
        var handle = _components.Add(value);
        _components.At(handle.Id).OnAdd();

        return ref Get(handle);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get(Handle<T> componentHandle) => ref _components[componentHandle];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Handle<T> componentHandle) => _components.IsValid(componentHandle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRemove(Handle<T> componentHandle)
    {
        _components.At(componentHandle.Id).OnDisable();
        _components.At(componentHandle.Id).OnRemove();
        return _components.Remove(componentHandle);
    }

    public void Update(GameTime gameTime)
    {
        if (!IsUpdateable) return;

        foreach (ref var handleItem in _components)
        {
            if (handleItem.Enabled)
                handleItem.Update(gameTime);
        }
    }

    public void LateUpdate(GameTime gameTime)
    {
        if (!IsLateUpdateable) return;

        foreach (ref var handleItem in _components)
        {
            if (handleItem.Enabled)
                handleItem.LateUpdate(gameTime);
        }
    }

    public void FixedUpdate(GameTime gameTime)
    {
        if (!IsFixedUpdateable) return;

        foreach (ref var handleItem in _components)
        {
            if (handleItem.Enabled)
                handleItem.FixedUpdate(gameTime);
        }
    }

    public void Draw(GameTime gameTime)
    {
        if (!IsDrawable) return;

        foreach (ref var handleItem in _components)
        {
            if (handleItem.Enabled)
                handleItem.Draw(gameTime);
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

    public void EntityActiveInHierarchyChanged(bool status, Handle handle)
    {
        ref var component = ref _components.At(handle.Id);

        if (!component.Enabled) return;

        if (component.Enabled)
        {
            if (status)
                component.OnEnable();
            else
                component.OnDisable();
        }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public HandleMapGrowing<T>.Enumerator GetEnumerator() => _components.GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateComponent(Handle handle, GameTime gameTime) => _components.At(handle.Id).Update(gameTime);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FixedUpdateComponent(Handle handle, GameTime gameTime) => _components.At(handle.Id).FixedUpdate(gameTime);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawComponent(Handle handle, GameTime gameTime) => _components.At(handle.Id).Draw(gameTime);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LateUpdateComponent(Handle handle, GameTime gameTime) => _components.At(handle.Id).LateUpdate(gameTime);
}
