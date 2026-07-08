using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int entityId, T value)
    {
        Set.Add(entityId);
        instances[Set.Index(entityId)] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get(int entityId) => ref instances[Set.Index(entityId)];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetIndex(int entity, out int index)
    {
        index = Set.Index(entity);

        return index < Set.Count && Set.Dense[index] == entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetByIndex(int index) => ref instances[index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(int entityId) => Set.Contains(entityId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveIfContains(int entityId)
    {
        if (Contains(entityId)) Remove(entityId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void Remove(int entityId) => Set.Remove(entityId);
}
