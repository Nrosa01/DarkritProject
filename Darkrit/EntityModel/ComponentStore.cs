using Darkrit.Base;
using Darkrit.DataStructures;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Darkrit.EntityModel;

public interface IComponentStore
{
    public void InitializePendingComponents();
    public void Update(GameTime gameTime);
    public void FixedUpdate(GameTime gameTime);
    public void Draw(GameTime gameTime);
}

public class ComponentStore<T>(int initialCapacity) : IComponentStore where T : struct, IComponent
{
    private static readonly bool IsUpdateable =
        typeof(T).IsDefined(typeof(UpdateableAttribute), inherit: false);

    private static readonly bool IsRenderable =
        typeof(T).IsDefined(typeof(RenderableAttribute), inherit: false);

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
        if (!IsUpdateable) return;

        foreach (ref var item in Components)
            item.Item.Update(gameTime);
    }

    public void FixedUpdate(GameTime gameTime)
    {
        if (!IsUpdateable) return;

        foreach (ref var handleItem in Components)
            handleItem.Item.FixedUpdate(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        if (!IsRenderable) return;

        foreach (ref var handleItem in Components)
            handleItem.Item.Draw(gameTime);
    }
}
