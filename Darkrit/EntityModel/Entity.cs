// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Darkrit.Base;
using Darkrit.DevTools.Logger;
using Microsoft.Xna.Framework;
using Handle = Darkrit.Base.Handle;

namespace Darkrit.EntityModel;

struct TypedHandle
{
    public Handle handle;
    public int type;

    public static TypedHandle Create<T>(Handle<T> handle) where T : struct, IComponent => new()
    {
        handle = new Handle
        {
            Id = handle.Id,
            Generation = handle.Generation
        },
        type = ComponentTypeId<T>.Id
    };
}

struct ComponentList
{
    List<TypedHandle> _handles = [];

    public readonly IReadOnlyList<TypedHandle> Components => _handles;
    public ComponentList()
    {
        _handles = [];
    }

    public readonly void Add<T>(Handle<T> handle) where T : struct, IComponent => _handles.Add(TypedHandle.Create(handle));

    public readonly Handle<T> Get<T>() where T : struct, IComponent
    {
        int id = ComponentTypeId<T>.Id;
        foreach (var item in _handles)
        {
            if (item.type == id)
                return new Handle<T>
                {
                    Id = item.handle.Id,
                    Generation = item.handle.Generation
                };
        }

        return Handle<T>.Default;
    }

    public readonly Handle Remove<T>() where T : struct, IComponent => Remove<T>(default, true);

    public readonly Handle Remove<T>(Handle<T> handle) where T : struct, IComponent => Remove<T>(handle, false);

    private readonly Handle Remove<T>(Handle<T> handle, bool onlyCheckType) where T : struct, IComponent
    {
        int id = ComponentTypeId<T>.Id;

        int toRemove = -1;

        for (int i = 0; i < _handles.Count; i++)
        {
            TypedHandle item = _handles[i];
            if (item.type == id && (onlyCheckType || (item.handle.Id == handle.Id && item.handle.Generation == handle.Generation)))
            {
                toRemove = i;
                break;
            }
        }

        // Swap and remove
        if (toRemove != -1)
        {
            var handleToRemove = _handles[toRemove];
            _handles[toRemove] = _handles[_handles.Count - 1];
            _handles.RemoveAt(_handles.Count - 1);

            return handleToRemove.handle;
        }

        return Handle.Default;
    }

    internal void Clear() => _handles.Clear();
}

public struct Entity
{
    public EntityRegistry World { get; init; }

    //readonly Dictionary<int, Handle<IComponent>> _componentIds = [];
    readonly ComponentList _componentList = new();

    internal readonly Handle<Entity> Handle { get; init; }

    /// <summary>
    /// Whether this Entity is active in the hierarchy
    /// </summary>
    public bool ActiveSelf { get; set; }

    /// <summary>
    /// Whether this Entity is active in the scene
    /// Say, Entity A has a child Entity B. B could be active but A 
    /// be inactive, which would result in B <see cref="ActiveInHierachy"/> be false
    /// while <see cref="ActiveSelf"/> is true
    /// </summary>
    public bool ActiveInHierachy { get; internal set; }

    public Transform Transform;

    public Vector2 Position
    {
        get => Transform.Position;
        set => Transform.Position = value;
    }

    public Entity()
    {
    }

    public Handle<T> AddComponent<T>() where T : struct, IComponent => AddComponent<T>(default);

    public Handle<T> AddComponent<T>(T component) where T : struct, IComponent
    {
        Handle<T> componentHandle = World.AddComponent(Handle, component);

        _componentList.Add<T>(componentHandle);

        return componentHandle;
    }

    public bool RemoveComponent<T>() where T : struct, IComponent
    {
        Handle removedHandle = _componentList.Remove<T>();
        if(removedHandle.Id == 0)
            return false;

        bool worldRemoves = World.RemoveComponent(ComponentTypeId<T>.Id, removedHandle);
        bool entityRemoves = removedHandle.Id != 0;

        if (!worldRemoves && entityRemoves)
            Log.Warning($"Component of types {typeof(T)} with handle {removedHandle} couldn't be removed from World but it was removed from entityt");

        return worldRemoves && entityRemoves;
    }

    public bool RemoveComponent<T>(Handle<T> handle) where T : struct, IComponent
    {
        bool worldRemoves = World.RemoveComponent<T>(handle);
        var removedHandle = _componentList.Remove<T>(handle);
        bool entityRemoves = removedHandle.Id != 0;

        if (worldRemoves && !entityRemoves)
            Log.Warning($"Component of types {typeof(T)} with handle {handle} couldn't be removed from entity but it could be removed from World");

        if (!worldRemoves && entityRemoves)
            Log.Warning($"Component of types {typeof(T)} with handle {handle} couldn't be removed from World but it was removed from entityt");

        return worldRemoves && entityRemoves;
    }

    public Handle<T> GetComponentHandle<T>() where T : struct, IComponent => _componentList.Get<T>();

    public ref T GetComponent<T>() where T : struct, IComponent => ref World.GetComponent<T>(GetComponentHandle<T>());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetComponent<T>(Handle<T> componentHandle) where T : struct, IComponent => ref World.GetComponent<T>(componentHandle);

    public bool HasComponent<T>() where T : struct, IComponent => GetComponentHandle<T>().Id != 0;

    // I have to ensure that out T is a reference and not a value
    public bool TryGetComponent<T>(out T component) where T : struct, IComponent
    {
        var handle = GetComponentHandle<T>();
        if (handle.Id == 0)
        {
            component = default;
            return false;
        }

        component = World.GetComponent<T>(GetComponentHandle<T>());
        return true;
    }

    internal readonly void Release()
    {
        foreach (var typedComponent in _componentList.Components)
            World.RemoveComponent(typedComponent.type, typedComponent.handle);

        _componentList.Clear();
    }
}
