using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Entity = System.Int32; // I should change this to a generational handle

/// Simple ECS implementation, mainly to understand how it works. Based on: 
/// https://gist.github.com/prime31/99c66a4aeb4fc0e75173d5ea80f75a97
/// https://gist.github.com/erodozer/2fe358f5dce36a0c9d6a7afc36c2adca
/// https://gist.github.com/f-space/f17529620fd772117b85c1b7208226ad
/// https://www.nvriezen.nl/tutorials/ecs-tutorial-part-1/
/// https://williamarnberg.com/articles/ecs_article/
/// https://austinmorlan.com/posts/entity_component_system/
/// https://github.com/skypjack/entt
/// https://github.com/itsBuggingMe/Frent

namespace MonoGameLibrary.TinyECS;

public static class TypeId
{
    private static int _nextId;

    public static int Next()
    {
        return Interlocked.Increment(ref _nextId) - 1;
    }
}

public static class TypeId<T>
{
    public static readonly int Id = TypeId.Next();
}

public class Registry(int maxEntities)
{
    readonly Dictionary<int, IComponentStore> data = [];
    Entity nextEntity = 0;

    public ComponentStore<T> Assure<T>()
    {
        var type = TypeId<T>.Id;
        if (data.TryGetValue(type, out var store)) return (ComponentStore<T>)data[type];

        var newStore = new ComponentStore<T>(maxEntities);
        data[type] = newStore;
        return newStore;
    }

    public Entity Create() => nextEntity++;

    public void Destroy(Entity entity)
    {
        foreach (var store in data.Values)
            store.RemoveIfContains(entity);
    }

    public void AddComponent<T>(Entity entity, T component) => Assure<T>().Add(entity, component);

    public ref T GetComponent<T>(Entity entity) => ref Assure<T>().Get(entity);

    public bool TryGetComponent<T>(Entity entity, ref T component)
    {
        var store = Assure<T>();
        if (store.Contains(entity))
        {
            component = store.Get(entity);
            return true;
        }

        return false;
    }

    public void RemoveComponent<T>(Entity entity) => Assure<T>().RemoveIfContains(entity);

    public delegate void TwoComponents<T, U>(ref T first, ref U second)
        where T : struct
        where U : struct;

    public void Query<T, U>(TwoComponents<T, U> action, bool runParallel = false) where T : struct where U : struct
    {
        var store1 = Assure<T>();
        var store2 = Assure<U>();
        var storeToUse = store1.Count > store2.Count ? store2.Set.Dense : store1.Set.Dense;
        Func<int, bool> contains;
        if (store1.Count > store2.Count)
            contains = store2.Contains;
        else
            contains = store1.Contains;

        if (runParallel)
        {
            storeToUse.AsParallel().ForAll(entity =>
            {
                if (!contains(entity)) return;
                action(ref store1.Get(entity), ref store2.Get(entity));
            });
        }
        else
        {
            foreach (var entity in storeToUse)
            {
                if (!contains(entity)) continue;
                action(ref store1.Get(entity), ref store2.Get(entity));
            }
        }
    }

    public delegate void ThreeComponents<T, U, V>(ref T first, ref U second, ref V third)
        where T : struct
        where U : struct
        where V : struct;

    public void Query<T, U, V>(ThreeComponents<T, U, V> action, bool runParallel = false)
        where T : struct
        where U : struct
        where V : struct
    {
        var store1 = Assure<T>();
        var store2 = Assure<U>();
        var store3 = Assure<V>();

        var storeToUse = store1.Set.Dense;
        var minCount = store1.Count;

        if (store2.Count < minCount)
        {
            minCount = store2.Count;
            storeToUse = store2.Set.Dense;
        }

        if (store3.Count < minCount)
        {
            storeToUse = store3.Set.Dense;
        }

        if (runParallel)
        {
            storeToUse.AsParallel().ForAll(entity =>
            {
                if (!store1.Contains(entity) ||
                    !store2.Contains(entity) ||
                    !store3.Contains(entity))
                    return;

                action(
                    ref store1.Get(entity),
                    ref store2.Get(entity),
                    ref store3.Get(entity));
            });
        }
        else
        {
            foreach (var entity in storeToUse)
            {
                if (!store1.Contains(entity) ||
                    !store2.Contains(entity) ||
                    !store3.Contains(entity))
                    continue;

                action(
                    ref store1.Get(entity),
                    ref store2.Get(entity),
                    ref store3.Get(entity));
            }
        }
    }

    public View<T> View<T>() => new(this);

    public View<T, U> View<T, U>() => new(this);

    public View<T, U, V> View<T, U, V>() => new(this);
}
