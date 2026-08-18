// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.Base;
using Darkrit.DataStructures;
using Microsoft.Xna.Framework;
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

    public static readonly int Count = ReflectionUtils.CountDerivedTypes<IComponent>();
}

public static class ComponentTypeId<T> where T : struct, IComponent
{
    public static readonly int Id = ComponentTypeId.Next();
}

public class EntityRegistry(int initialCapacity)
{
    private readonly IComponentStore[] _componentStores = new IComponentStore[ComponentTypeId.Count];
    private readonly HandleMapGrowing<Entity> _entities = new(initialCapacity);

    public ref Entity GetEntity(Handle<Entity> entityHandle) => ref _entities[entityHandle];

    public ref readonly Entity GetEntityReadonly(Handle<Entity> entityHandle) => ref _entities.GetReadonly(entityHandle);


    public EntityRegistry() : this(1000) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ComponentStore<T> GetStore<T>() where T : struct, IComponent => (ComponentStore<T>)(_componentStores[ComponentTypeId<T>.Id] ??= new ComponentStore<T>(initialCapacity));

    public Handle<Entity> CreateEntity()
    {
        return _entities.Add(new Entity
        {
            World = this,
            Handle = _entities.PeekNextHandle()
        });
    }

    public bool Destroy(Handle<Entity> entity)
    {
        _entities[entity].Release();
        return _entities.Remove(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Exists(Handle<Entity> entityHandle) => _entities.IsValid(entityHandle);

    internal Handle<T> AddComponent<T>(Handle<Entity> entityHandle, T component) where T : struct, IComponent
    {
        component.World = this;
        component.EntityHandle = entityHandle;
        return GetStore<T>().Add(component);
    }

    internal bool RemoveComponent<T>(Handle<Entity> entityHandle, Handle<T> component) where T : struct, IComponent
    {
        return GetStore<T>().TryRemove(component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T GetComponent<T>(Handle<T> componentHandle) where T : struct, IComponent => ref GetStore<T>().Get(componentHandle);

    internal bool RemoveComponent<T>(Handle<T> componentHandle) where T : struct, IComponent => GetStore<T>().TryRemove(componentHandle);

    internal bool RemoveComponent(int typeId, Handle<IComponent> iComponent) => _componentStores[typeId].TryRemove(iComponent);
    internal bool RemoveComponent(int typeId, Handle iComponent) => _componentStores[typeId].TryRemove(iComponent);

    public void Update(GameTime gameTime)
    {
        foreach (var store in _componentStores)
        {
            store?.InitializePendingComponents();
            store?.Update(gameTime);
        }
    }

    public void FixedUpdate(GameTime gameTime)
    {
        foreach (var store in _componentStores)
            store?.FixedUpdate(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        foreach (var store in _componentStores)
            store?.Draw(gameTime);
    }
}
