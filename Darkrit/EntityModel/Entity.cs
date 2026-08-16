// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.Base;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Darkrit.EntityModel;

public struct Entity
{
    public EntityRegistry World { get; init; }

    readonly Dictionary<int, Handle<IComponent>> _componentIds = [];

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

        _componentIds[ComponentTypeId<T>.Id] = new Handle<IComponent>
        {
            Id = componentHandle.Id,
            Generation = componentHandle.Generation
        };

        return componentHandle;
    }

    public Handle<T> GetComponentHandle<T>() where T : struct, IComponent
    {
        if (_componentIds.TryGetValue(ComponentTypeId<T>.Id, out Handle<IComponent> handle))
            return new Handle<T>
            {
                Id = handle.Id,
                Generation = handle.Generation
            };

        return Handle<T>.Default;
    }

    public ref T GetComponent<T>() where T : struct, IComponent => ref World.GetComponent<T>(GetComponentHandle<T>());

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
        foreach (var pair in _componentIds)
            World.RemoveComponent(pair.Key, pair.Value);

        _componentIds.Clear();
    }
}
