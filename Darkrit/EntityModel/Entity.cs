// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.Base;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Darkrit.EntityModel;

public struct Entity
{
    public EntityRegistry World { get; init; }

    readonly Dictionary<int, Handle<IComponent>> componentIds = [];

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

    public Handle<T> AddComponent<T>()  where T : struct, IComponent
    {
        Handle<T> componentHandle = World.AddComponent<T>(Handle, default);

        if (componentHandle is Handle<IComponent> icomponentHandle)
                componentIds[ComponentTypeId<T>.Id] = icomponentHandle;

        return componentHandle;
    }

    public Handle<T> AddComponent<T>(T component) where T : struct, IComponent
    {
        component.EntityHandle = Handle;
        component.World = World;

        Handle<T> componentHandle = World.AddComponent(Handle, component);

        if (componentHandle is Handle<IComponent> icomponentHandle)
            componentIds[ComponentTypeId<T>.Id] = icomponentHandle;

        return componentHandle;
    }

    public Handle<T> GetComponentHandle<T>() where T : struct, IComponent
    {
        if (componentIds[ComponentTypeId<T>.Id] is Handle<T> handle)
            return handle;

        return Handle<T>.Default;
    }

    public ref T GetComponent<T>() where T : struct, IComponent => ref World.GetComponent<T>(GetComponentHandle<T>());

    public bool HasComponent<T>() where T : struct, IComponent => GetComponentHandle<T>().Id != 0;

    public bool TryGetComponent<T>(out T component) where T : struct, IComponent
    {
        var handle = GetComponentHandle<T>();
        if(handle.Id == 0)
        {
            component = default;
            return false;
        }

        component = World.GetComponent<T>(GetComponentHandle<T>());
        return true;
    }
}
