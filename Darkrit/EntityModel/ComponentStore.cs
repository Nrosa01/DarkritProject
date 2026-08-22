// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Darkrit.Base;
using Darkrit.DataStructures;
using Microsoft.Xna.Framework;

namespace Darkrit.EntityModel;

/// <summary>
/// Interface for the container that holds component of a specified type
/// </summary>
public interface IComponentStore
{
    /// <summary>
    /// Currently unused, initialized recently created componentes
    /// </summary>
    public void InitializePendingComponents();
    
    /// <summary>
    /// Calls every component <see cref="IComponent.Update(GameTime)"/> if they're enabled
    /// </summary>
    /// <param name="gameTime"></param>
    public void Update(GameTime gameTime);

    /// <summary>
    /// Calls every component <see cref="IComponent.FixedUpdate(GameTime)"/> if they're enabled
    /// </summary>
    /// <param name="gameTime"></param>
    public void FixedUpdate(GameTime gameTime);

    /// <summary>
    /// Calls every component <see cref="IComponent.LateUpdate(GameTime)"/> if they're enabled
    /// Runs after both <see cref="Update(GameTime)"/> and <see cref="FixedUpdate(GameTime)"/>
    /// </summary>
    /// <param name="gameTime"></param>
    void LateUpdate(GameTime gameTime);


    /// <summary>
    /// Calls every component <see cref="IComponent.Draw(GameTime)"/> if they're enabled
    /// </summary>
    /// <param name="gameTime"></param>
    public void Draw(GameTime gameTime);

    /// <summary>
    /// Removes a component given a handle.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns>True if the component was removed</returns>
    public bool TryRemove(Handle<IComponent> handle);
    
    /// <summary>
    /// Removes a component given an untyped handle.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns>True if the component was removed</returns>
    public bool TryRemove(Handle handle);

    /// <summary>
    /// Should not be called directly. This callback is for when
    /// an entity is enabled or disabled, to inform its components
    /// </summary>
    /// <param name="status"></param>
    /// <param name="handle"></param>
    public void EntityActiveInHierarchyChanged(bool status, Handle handle);

    /// <summary>
    /// Updates an specific component given an untyped handle
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="gameTime"></param>
    void UpdateComponent(Handle handle, GameTime gameTime);
    
    /// <summary>
    /// FixesUpdate a specific component given an untyped handle
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="gameTime"></param>
    public void FixedUpdateComponent(Handle handle, GameTime gameTime);
    
    /// <summary>
    /// Draws a specific component given an untyped handle
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="gameTime"></param>
    void DrawComponent(Handle handle, GameTime gameTime);
    
    /// <summary>
    /// LateUpdate a component given a untyped handle
    /// </summary>
    /// <param name="gameTime"></param>
    void LateUpdateComponent(Handle handle, GameTime gameTime);

    /// <summary>
    /// Whether the component is Updeable
    /// </summary>
    bool IsUpdateable { get; }

    /// <summary>
    /// Whether the component is FixedUpdateable
    /// </summary>
    bool IsFixedUpdateable { get; }

    /// <summary>
    /// Whether the component is Drawable
    /// </summary>
    bool IsDrawable { get; }

    /// <summary>
    /// Priority of the component execution. Lower values means more priority
    /// </summary>
    int Priority { get; }
}

/// <summary>
/// Storage for components of a specific type
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="initialCapacity"></param>
public class ComponentStore<T>(int initialCapacity) : IComponentStore, IEnumerable<T> where T : struct, IComponent, IHandle<T>
{
    private static readonly bool IsUpdateable = typeof(T).IsDefined(typeof(UpdateableAttribute), inherit: false);
    
    private static readonly bool IsLateUpdateable = typeof(T).IsDefined(typeof(LateUpdateableAttribute), inherit: false);

    private static readonly bool IsFixedUpdateable = typeof(T).IsDefined(typeof(FixedUpdateableAttribute), inherit: false);

    private static readonly bool IsDrawable = typeof(T).IsDefined(typeof(DrawableAttribute), inherit: false);
    
    private static readonly bool OverridesPriority = typeof(T).IsDefined(typeof(PriorityAttribute), inherit: false);
    
    internal static readonly int Priority = OverridesPriority ? typeof(T).GetCustomAttribute<PriorityAttribute>(inherit: false).Priority : 0;

    /// <inheritdoc/>
    bool IComponentStore.IsUpdateable => IsUpdateable;

    /// <inheritdoc/>
    bool IComponentStore.IsFixedUpdateable => IsFixedUpdateable;

    /// <inheritdoc/>
    bool IComponentStore.IsDrawable => IsDrawable;

    private readonly HandleMapGrowing<T> _components = new(initialCapacity);
    private Stack<Handle<T>> nonInitializedComponents = new();

    /// <summary>
    /// Amount of components in use
    /// </summary>
    public int Count => _components.Count;

    /// <inheritdoc/>
    int IComponentStore.Priority => Priority;

    
    /// <inheritdoc/>
    public void InitializePendingComponents()
    {
        while (nonInitializedComponents.TryPop(out Handle<T> handle))
            _components[handle].OnAdd();
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Add(T value)
    {
        var handle = _components.Add(value);
        _components.At(handle.Id).OnAdd();

        return ref Get(handle);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get(Handle<T> componentHandle) => ref _components[componentHandle];

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Handle<T> componentHandle) => _components.IsValid(componentHandle);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRemove(Handle<T> componentHandle)
    {
        _components.At(componentHandle.Id).OnDisable();
        _components.At(componentHandle.Id).OnRemove();
        return _components.Remove(componentHandle);
    }

    /// <inheritdoc/>
    public void Update(GameTime gameTime)
    {
        if (!IsUpdateable) return;

        foreach (ref var handleItem in _components)
        {
            if (handleItem.Enabled)
                handleItem.Update(gameTime);
        }
    }

    /// <inheritdoc/>
    public void LateUpdate(GameTime gameTime)
    {
        if (!IsLateUpdateable) return;

        foreach (ref var handleItem in _components)
        {
            if (handleItem.Enabled)
                handleItem.LateUpdate(gameTime);
        }
    }

    /// <inheritdoc/>
    public void FixedUpdate(GameTime gameTime)
    {
        if (!IsFixedUpdateable) return;

        foreach (ref var handleItem in _components)
        {
            if (handleItem.Enabled)
                handleItem.FixedUpdate(gameTime);
        }
    }

    /// <inheritdoc/>
    public void Draw(GameTime gameTime)
    {
        if (!IsDrawable) return;

        foreach (ref var handleItem in _components)
        {
            if (handleItem.Enabled)
                handleItem.Draw(gameTime);
        }
    }

    /// <inheritdoc/>
    public bool TryRemove(Handle<IComponent> handle)
    {
        return TryRemove(new Handle<T>
        {
            Id = handle.Id,
            Generation = handle.Generation
        });
    }

    /// <inheritdoc/>
    public bool TryRemove(Handle handle)
    {
        return TryRemove(new Handle<T>
        {
            Id = handle.Id,
            Generation = handle.Generation
        });
    }

    /// <inheritdoc/>
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
    /// <inheritdoc/>
    public HandleMapGrowing<T>.Enumerator GetEnumerator() => _components.GetEnumerator();

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateComponent(Handle handle, GameTime gameTime) => _components.At(handle.Id).Update(gameTime);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FixedUpdateComponent(Handle handle, GameTime gameTime) => _components.At(handle.Id).FixedUpdate(gameTime);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawComponent(Handle handle, GameTime gameTime) => _components.At(handle.Id).Draw(gameTime);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LateUpdateComponent(Handle handle, GameTime gameTime) => _components.At(handle.Id).LateUpdate(gameTime);
}
