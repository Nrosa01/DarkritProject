// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

/// Simple ECS implementation, mainly to understand how it works. Based on: 
/// https://gist.github.com/prime31/99c66a4aeb4fc0e75173d5ea80f75a97
/// https://gist.github.com/erodozer/2fe358f5dce36a0c9d6a7afc36c2adca
/// https://gist.github.com/f-space/f17529620fd772117b85c1b7208226ad
/// https://www.nvriezen.nl/tutorials/ecs-tutorial-part-1/
/// https://williamarnberg.com/articles/ecs_article/
/// https://austinmorlan.com/posts/entity_component_system/
/// https://github.com/skypjack/entt
/// https://github.com/itsBuggingMe/Frent

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Darkrit.Base;

namespace Darkrit.TinyECS;

public interface IComponent
{

}

public record struct Entity(Int32 Id, Int32 Generation);

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

public partial class Registry
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

    public Registry(int maxEntities)
    {
        var typeCount = ReflectionUtils.CountDerivedTypes<IComponent>();
        data = new IComponentStore[typeCount];

        generations = new int[maxEntities];
        this.maxEntities = maxEntities;
    }

    public Registry() : this(1000) { }

    public ComponentStore<T> GetStore<T>() => (ComponentStore<T>)(data[ComponentTypeId<T>.Id] ??= new ComponentStore<T>(maxEntities));

    public Entity Create()
    {
        var next = NextEntityId();
        return new() { Id = next, Generation = generations[next] };
    }

    public bool TryDestroy(Entity entity)
    {
        if (!Exists(entity)) return false;

        foreach (var store in data)
            store.RemoveIfContains(entity.Id);

        deletedEntities.Push(entity.Id);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Exists(Entity entity) => generations[entity.Id] == entity.Generation;

    public void AddComponent<T>(Entity entity, T component) => GetStore<T>().Add(entity.Id, component);

    public ref T GetComponent<T>(Entity entity) => ref GetStore<T>().Get(entity.Id);

    public bool TryGetComponent<T>(Entity entity, ref T component)
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

    public void RemoveComponent<T>(Entity entity) => GetStore<T>().RemoveIfContains(entity.Id);
}
