using System;
using System.Collections.Generic;
using System.Text;

namespace MonoGameLibrary.TinyECS;

public interface IComponentStore
{
    void RemoveIfContains(int entityId);
}

public class ComponentStore<T>(int maxComponents) : IComponentStore
{
    public readonly SparseSet Set = new(maxComponents);
    private readonly T[] instances = new T[maxComponents];

    public int Count => Set.Count;

    public void Add(int entityId, T value)
    {
        Set.Add(entityId);
        instances[Set.Index(entityId)] = value;
    }

    public ref T Get(int entityId) => ref instances[Set.Index(entityId)];

    public bool Contains(int entityId) => Set.Contains(entityId);

    public void RemoveIfContains(int entityId)
    {
        if (Contains(entityId)) Remove(entityId);
    }

    void Remove(int entityId) => Set.Remove(entityId);
}
