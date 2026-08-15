// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.Base;
using Darkrit.TinyECS;
using System;
using System.Collections.Generic;
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

    public static int Count => _nextId - 1;
}

public static class ComponentTypeId<T>
{
    public static readonly int Id = ComponentTypeId.Next();
}

public class EntityRegistry
{
    private readonly IComponentStore[] data;
    private readonly Stack<int> deletedEntities = new();
    private readonly int maxEntities;
    private readonly int[] generations;
    private Int32 nextEntity = 0;

    private Int32 NextEntityId()
    {
        if (deletedEntities.TryPop(out var result))
            return result;
        else
            return (++nextEntity % maxEntities);
    }

    public EntityRegistry(int maxEntities)
    {
        var typeCount = ReflectionUtils.CountDerivedTypes<IComponent>();
        data = new IComponentStore[typeCount];

        generations = new int[maxEntities];
        this.maxEntities = maxEntities;
    }

    public ref Entity GetEntity(Handle<Entity> entityHandle)
    {

    }

    public EntityRegistry() : this(1000) { }

    public ComponentStore<T> GetStore<T>() => (ComponentStore<T>)(data[ComponentTypeId<T>.Id] ??= new ComponentStore<T>(maxEntities));

    public EntityId Create()
    {
        var next = NextEntityId();
        return new() { Id = next, Generation = generations[next] };
    }

    public bool TryDestroy(EntityId entity)
    {
        if (!Exists(entity)) return false;

        foreach (var store in data)
            store.RemoveIfContains(entity.Id);

        deletedEntities.Push(entity.Id);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Exists(EntityId entity) => generations[entity.Id] == entity.Generation;

    public void AddComponent<T>(EntityId entity, T component) => GetStore<T>().Add(entity.Id, component);

    //public ref T GetComponent<T>(EntityId entity) => ref GetStore<T>().Get(entity.Id);
    public ref T GetComponent<T>(Handle<T> componentHandle) => ref GetStore<T>().Get(componentHandle);

    public bool TryGetComponent<T>(EntityId entity, ref T component)
    {
        if (!Exists(entity)) return false;

        var store = GetStore<T>();
        if (store.Contains(entity.Id))
        {
            component = store.Get(entity.Id);
            return true;
        }

        return false;
    }

    public void RemoveComponent<T>(EntityId entity) => GetStore<T>().RemoveIfContains(entity.Id);
}
