// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.Base;
using Darkrit.DataStructures;
using Darkrit.TinyECS;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Darkrit.EntityModel;

public static class ComponentTypeId
{
    private static int _nextId;

    public static int Next()
    {
        return Interlocked.Increment(ref _nextId) - 1;
    }

    public static readonly int Count = ReflectionUtils.CountDerivedTypes<IComponent>();
}

public static class ComponentTypeId<T> where T : struct, IComponent
{
    public static readonly int Id = ComponentTypeId.Next();
}

public class EntityRegistry(int initialCapacity)
{
    private readonly IComponentStore[] _data = new IComponentStore[ComponentTypeId.Count];
    private readonly HandleMapGrowing<Entity> _entities = new(initialCapacity);


    public ref Entity GetEntity(Handle<Entity> entityHandle) => ref _entities[entityHandle];

    public EntityRegistry() : this(1000) { }

    public ComponentStore<T> GetStore<T>() where T : struct, IComponent => (ComponentStore<T>)(_data[ComponentTypeId<T>.Id] ??= new ComponentStore<T>(initialCapacity));

    public Handle<Entity> Create() => _entities.Add(new Entity {
        World = this
    });

    public bool TryDestroyImmediate(Handle<Entity> entity) => _entities.Remove(entity);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Exists(Handle<Entity> entityHandle) => _entities.IsValid(entityHandle);

    public Handle<T> AddComponent<T>() where T : struct, IComponent  => GetStore<T>().Add(default);
    public Handle<T> AddComponent<T>(T component) where T : struct, IComponent  => GetStore<T>().Add(component);

    //public ref T GetComponent<T>(EntityId entity) => ref GetStore<T>().Get(entity.Id);
    public ref T GetComponent<T>(Handle<T> componentHandle) where T : struct, IComponent => ref GetStore<T>().Get(componentHandle);

    public bool RemoveComponent<T>(Handle<T> componentHandle) where T : struct, IComponent => GetStore<T>().TryRemove(componentHandle);
}
