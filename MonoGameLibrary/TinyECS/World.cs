using System;
using System.Collections.Generic;
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

public class World(int maxEntities)
{
    readonly Dictionary<Type, IComponentStore> data = [];
    Entity nextEntity = 0;

    public ComponentStore<T> Assure<T>()
    {
        var type = typeof(T);
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

    public View<T> View<T>() => new(this);

    public View<T, U> View<T, U>() => new(this);

    public View<T, U, V> View<T, U, V>() => new(this);
}
