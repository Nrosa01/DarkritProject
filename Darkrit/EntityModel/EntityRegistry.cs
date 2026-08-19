// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Darkrit.Base;
using Darkrit.DataStructures;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;

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

public class EntityRegistry(int initialCapacity) : IEnumerable<Entity>, IEnumerable<HandleItem<Entity>>
{
    private readonly IComponentStore[] _componentStores = new IComponentStore[ComponentTypeId.Count];
    private readonly HandleMapGrowing<Entity> _entities = new(initialCapacity);

    public int Count => _entities.Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Entity GetEntity(Handle<Entity> entityHandle) => ref _entities[entityHandle];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref readonly Entity GetEntityReadonly(Handle<Entity> entityHandle) => ref _entities.GetReadonly(entityHandle);


    public EntityRegistry() : this(1000) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ComponentStore<T> GetStore<T>() where T : struct, IComponent => (ComponentStore<T>)(_componentStores[ComponentTypeId<T>.Id] ??= new ComponentStore<T>(initialCapacity));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<Entity> CreateEntityByHandle()
    {
        return _entities.Add(new Entity
        {
            World = this,
            Handle = _entities.PeekNextHandle(),
            ActiveSelf = true,
            ActiveInHierachy = true,
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<Entity> CreateEntityByHandle(StringID name)
    {
        return _entities.Add(new Entity
        {
            NameID = name,
            World = this,
            Handle = _entities.PeekNextHandle(),
            ActiveSelf = true,
            ActiveInHierachy = true,
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Entity CreateEntity()
    {
        Handle<Entity> handle = CreateEntityByHandle();
        return ref GetEntity(handle);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Entity CreateEntity(Handle<Entity> parentHandle)
    {
        ref Entity entity = ref CreateEntity();
        entity.SetParent(parentHandle);
        return ref entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Entity CreateEntity(ref Entity parent)
    {
        ref Entity entity = ref CreateEntity();
        entity.SetParent(parent.Handle);
        return ref entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<Entity> CreateEntityByHandle(Handle<Entity> parent)
    {
        var handle = CreateEntityByHandle();
        GetEntity(handle).SetParent(parent);
        return handle;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<Entity> CreateEntityByHandle(Handle<Entity> parent, StringID name)
    {
        var handle = CreateEntityByHandle(name);
        GetEntity(handle).SetParent(parent);
        return handle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<Entity> CreateEntityByHandle(ref Entity parent)
    {
        var handle = CreateEntityByHandle();
        GetEntity(handle).SetParent(parent.Handle);
        return handle;
    }

    public bool RemoveEntity(Handle<Entity> handle)
    {
        if (!_entities.IsValid(handle))
            return false;

        var child = _entities[handle]._firstChild;

        while (child.Id != 0)
        {
            var next = _entities[child]._nextSibling;

            RemoveEntity(child);

            child = next;
        }

        _entities[handle].Release();

        return _entities.Remove(handle);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Exists(Handle<Entity> entityHandle) => _entities.IsValid(entityHandle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Handle<T> CreateComponent<T>(Handle<Entity> entityHandle, T component) where T : struct, IComponent => GetStore<T>().Add(component);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool RemoveComponent<T>(Handle<Entity> entityHandle, Handle<T> component) where T : struct, IComponent => GetStore<T>().TryRemove(component);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T GetComponent<T>(Handle<T> componentHandle) where T : struct, IComponent => ref GetStore<T>().Get(componentHandle);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool RemoveComponent<T>(Handle<T> componentHandle) where T : struct, IComponent => GetStore<T>().TryRemove(componentHandle);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool RemoveComponent(int typeId, Handle<IComponent> iComponent) => _componentStores[typeId].TryRemove(iComponent);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    IEnumerator<HandleItem<Entity>> IEnumerable<HandleItem<Entity>>.GetEnumerator() => GetEnumerator();
    IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public HandleMapGrowing<Entity>.Enumerator GetEnumerator() => _entities.GetEnumerator();

    public bool IsValid(Handle<Entity> entity) => _entities.IsValid(entity);

    public void EditorDraw()
    {
        ImGui.Begin("Entities");
        
        void DrawEntity(Handle<Entity> handle)
        {
            ref Entity entity = ref GetEntity(handle);

            bool hasChildren = entity._firstChild.Id != 0;

            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.OpenOnArrow;

            if (hasChildren)
                flags |= ImGuiTreeNodeFlags.DefaultOpen;
            else
                flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;

            bool open = ImGui.TreeNodeEx(entity.Name, flags);

            if (ImGui.IsItemClicked())
            {
                // I need to think where to show components
            }

            if (open && hasChildren)
            {
                foreach (var child in entity.Children)
                    DrawEntity(child.Handle);

                ImGui.TreePop();
            }
        }

        foreach (ref HandleItem<Entity> item in this)
        {
            if (item.Item._parent.Id == 0)
                DrawEntity(item.Item.Handle);
        }

        ImGui.End();
    }
}
