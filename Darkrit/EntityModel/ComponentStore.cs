using Darkrit.Base;
using Darkrit.DataStructures;
using Darkrit.TinyECS;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Darkrit.EntityModel;

public interface IComponentStore {
    public void InitializePendingComponents();
    public void Update(GameTime gameTime);
    public void FixedUpdate(GameTime gameTime);
    public void Draw(GameTime gameTime);
}

public class ComponentStore<T>(int initialCapacity) : IComponentStore where T : struct, IComponent
{
    public readonly HandleMapGrowing<T> Components = new(initialCapacity);
    private Stack<Handle<T>> nonInitializedComponents = new();

    public int Count => Components.Count;

    public void InitializePendingComponents()
    {
        while (nonInitializedComponents.TryPop(out Handle<T> handle))
            Components[handle].Start();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<T> Add(T value)
    {
        var handle = Components.Add(value);
        nonInitializedComponents.Push(handle);
        return handle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get(Handle<T> componentHandle) => ref Components[componentHandle];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Handle<T> componentHandle) => Components.IsValid(componentHandle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRemove(Handle<T> componentHandle) => Components.Remove(componentHandle);

    public void Update(GameTime gameTime)
    {
        foreach (ref var item in Components)
        {
            item.Item.Update(gameTime);
        }
    }

    public void FixedUpdate(GameTime gameTime)
    {
        foreach (ref var item in Components)
        {
            item.Item.FixedUpdate(gameTime);
        }
    }

    public void Draw(GameTime gameTime)
    {
        foreach (ref var item in Components)
        {
            item.Item.Draw(gameTime);
        }
    }
}
