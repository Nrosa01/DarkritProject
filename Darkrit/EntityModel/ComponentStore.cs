using Darkrit.Base;
using Darkrit.DataStructures;
using Darkrit.TinyECS;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Darkrit.EntityModel;

public interface IComponentStore { }

public class ComponentStore<T>(int initialCapacity) : IComponentStore where T : struct, IComponent
{
    public readonly HandleMapGrowing<T> Components = new(initialCapacity);

    public int Count => Components.Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<T> Add(T value) => Components.Add(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get(Handle<T> componentHandle) => ref Components[componentHandle];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Handle<T> componentHandle) => Components.IsValid(componentHandle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryRemove(Handle<T> componentHandle) => Components.Remove(componentHandle);
}
