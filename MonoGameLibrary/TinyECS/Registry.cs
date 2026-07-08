using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    public void Query<T, U>(TwoComponents<T, U> action)
        where T : struct
        where U : struct
    {
        var store1 = Assure<T>();
        var store2 = Assure<U>();

        if (store1.Count <= store2.Count)
        {
            var dense = store1.Set.Dense;
            var count = store1.Count;

            for (int i = 0; i < count; i++)
            {
                var entity = dense[i];

                if (!store2.TryGetIndex(entity, out var index2))
                    continue;

                action(
                    ref store1.GetByIndex(i),
                    ref store2.GetByIndex(index2));
            }
        }
        else
        {
            var dense = store2.Set.Dense;
            var count = store2.Count;

            for (int i = 0; i < count; i++)
            {
                var entity = dense[i];

                if (!store1.TryGetIndex(entity, out var index1))
                    continue;

                action(
                    ref store1.GetByIndex(index1),
                    ref store2.GetByIndex(i));
            }
        }
    }

    public void QueryParallel<T, U>(TwoComponents<T, U> action)
    where T : struct
    where U : struct
    {
        var store1 = Assure<T>();
        var store2 = Assure<U>();

        if (store1.Count <= store2.Count)
        {
            var dense = store1.Set.Dense;
            var count = store1.Count;

            Parallel.For(0, count, i =>
            {
                var entity = dense[i];

                if (!store2.TryGetIndex(entity, out var index2))
                    return;

                action(
                    ref store1.GetByIndex(i),
                    ref store2.GetByIndex(index2));
            });
        }
        else
        {
            var dense = store2.Set.Dense;
            var count = store2.Count;

            Parallel.For(0, count, i =>
            {
                var entity = dense[i];

                if (!store1.TryGetIndex(entity, out var index1))
                    return;

                action(
                    ref store1.GetByIndex(index1),
                    ref store2.GetByIndex(i));
            });
        }
    }

    public delegate void ThreeComponents<T, U, V>(ref T first, ref U second, ref V third)
        where T : struct
        where U : struct
        where V : struct;

    public void Query<T, U, V>(ThreeComponents<T, U, V> action)
        where T : struct
        where U : struct
        where V : struct
    {
        var store1 = Assure<T>();
        var store2 = Assure<U>();
        var store3 = Assure<V>();

        var leader = 0;
        var leaderCount = store1.Count;

        if (store2.Count < leaderCount)
        {
            leader = 1;
            leaderCount = store2.Count;
        }

        if (store3.Count < leaderCount)
        {
            leader = 2;
            leaderCount = store3.Count;
        }

        switch (leader)
        {
            case 0:
                {
                    var dense = store1.Set.Dense;

                    for (int i = 0; i < leaderCount; i++)
                    {
                        var entity = dense[i];

                        if (!store2.TryGetIndex(entity, out var index2) ||
                            !store3.TryGetIndex(entity, out var index3))
                            continue;

                        action(
                            ref store1.GetByIndex(i),
                            ref store2.GetByIndex(index2),
                            ref store3.GetByIndex(index3));
                    }

                    break;
                }

            case 1:
                {
                    var dense = store2.Set.Dense;

                    for (int i = 0; i < leaderCount; i++)
                    {
                        var entity = dense[i];

                        if (!store1.TryGetIndex(entity, out var index1) ||
                            !store3.TryGetIndex(entity, out var index3))
                            continue;

                        action(
                            ref store1.GetByIndex(index1),
                            ref store2.GetByIndex(i),
                            ref store3.GetByIndex(index3));
                    }

                    break;
                }

            case 2:
                {
                    var dense = store3.Set.Dense;

                    for (int i = 0; i < leaderCount; i++)
                    {
                        var entity = dense[i];

                        if (!store1.TryGetIndex(entity, out var index1) ||
                            !store2.TryGetIndex(entity, out var index2))
                            continue;

                        action(
                            ref store1.GetByIndex(index1),
                            ref store2.GetByIndex(index2),
                            ref store3.GetByIndex(i));
                    }

                    break;
                }
        }
    }

    public void QueryParallel<T, U, V>(ThreeComponents<T, U, V> action)
        where T : struct
        where U : struct
        where V : struct
    {
        var store1 = Assure<T>();
        var store2 = Assure<U>();
        var store3 = Assure<V>();

        var leader = 0;
        var leaderCount = store1.Count;

        if (store2.Count < leaderCount)
        {
            leader = 1;
            leaderCount = store2.Count;
        }

        if (store3.Count < leaderCount)
        {
            leader = 2;
            leaderCount = store3.Count;
        }

        switch (leader)
        {
            case 0:
                {
                    var dense = store1.Set.Dense;

                    Parallel.For(0, leaderCount, i =>
                    {
                        var entity = dense[i];

                        if (!store2.TryGetIndex(entity, out var index2) ||
                            !store3.TryGetIndex(entity, out var index3))
                            return;

                        action(
                            ref store1.GetByIndex(i),
                            ref store2.GetByIndex(index2),
                            ref store3.GetByIndex(index3));
                    });

                    break;
                }

            case 1:
                {
                    var dense = store2.Set.Dense;

                    Parallel.For(0, leaderCount, i =>
                    {
                        var entity = dense[i];

                        if (!store1.TryGetIndex(entity, out var index1) ||
                            !store3.TryGetIndex(entity, out var index3))
                            return;

                        action(
                            ref store1.GetByIndex(index1),
                            ref store2.GetByIndex(i),
                            ref store3.GetByIndex(index3));
                    });

                    break;
                }

            case 2:
                {
                    var dense = store3.Set.Dense;

                    Parallel.For(0, leaderCount, i =>
                    {
                        var entity = dense[i];

                        if (!store1.TryGetIndex(entity, out var index1) ||
                            !store2.TryGetIndex(entity, out var index2))
                            return;

                        action(
                            ref store1.GetByIndex(index1),
                            ref store2.GetByIndex(index2),
                            ref store3.GetByIndex(i));
                    });

                    break;
                }
        }
    }
}
