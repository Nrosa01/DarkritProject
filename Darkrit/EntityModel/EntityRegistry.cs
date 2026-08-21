// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.Base;
using Darkrit.DataStructures;
using Darkrit.DevTools.Logger;
using Darkrit.Physics.Boxy2D;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
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

public class EntityRegistry(int initialCapacity) : IEnumerable<Entity>, IEnumerable<HandleItem<Entity>>
{
    private readonly IComponentStore[] _componentStores = new IComponentStore[ComponentTypeId.Count];
    private readonly HandleMapGrowing<Entity> _entities = new(initialCapacity);

    private readonly GrowableArray<TypedHandle> _updateNodes = [];
    private readonly GrowableArray<TypedHandle> _fixedUpdateNodes = [];
    private readonly GrowableArray<TypedHandle> _drawNodes = [];

    bool _useHierachyScheduler;
    /// <summary>
    /// If true, means the hierachy will update components based
    /// on the hierarchy order, useful if you need components from parent entity 
    /// to update before children
    /// 
    /// If component order doesn't matter, better disable this for performance
    /// </summary>
    public bool UseHierarchyScheduler
    {
        get => _useHierachyScheduler;
        set
        {
            if (_useHierachyScheduler == value) return;

            _useHierachyScheduler = value;
            if (!_useHierachyScheduler)
                ClearUpdateLists();
            else
                MarkHierarchyDirty();
        }
    }

    bool _isDirty;

    private void UpdateComponentUpdateLists()
    {
        Log.Info("Updating Component Lists");

        ClearUpdateLists();

        foreach (ref var item in this)
        {
            // Top level
            if (!item.Item.HasParent)
            {
                TraverseHierarchy(item.Handle, (ref entity) =>
                {
                    foreach (var typedComponent in entity.Components)
                    {
                        if (_componentStores[typedComponent.type].IsUpdateable)
                            _updateNodes.Add(typedComponent);

                        if (_componentStores[typedComponent.type].IsFixedUpdateable)
                            _fixedUpdateNodes.Add(typedComponent);

                        if (_componentStores[typedComponent.type].IsDrawable)
                            _drawNodes.Add(typedComponent);
                    }
                });
            }
        }

        //Log.Info($""" 
        //Amount of update nodes is {_updateNodes.Count}
        //Amount of fixed update nodes is {_fixedUpdateNodes.Count}
        //Amount of drawable nodes is {_drawNodes.Count}
        //""");
    }

    private void ClearUpdateLists()
    {
        _updateNodes.Clear();
        _fixedUpdateNodes.Clear();
        _drawNodes.Clear();
    }

    internal void MarkHierarchyDirty() => _isDirty = true;

    private void OrderHierachyIfDirty()
    {
        if (_isDirty)
        { 
            UpdateComponentUpdateLists();
            _isDirty = false;
        }
    }

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
        entity.TrySetParent(parentHandle);
        return ref entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Entity CreateEntity(ref Entity parent)
    {
        ref Entity entity = ref CreateEntity();
        entity.TrySetParent(parent.Handle);
        return ref entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<Entity> CreateEntityByHandle(Handle<Entity> parent)
    {
        var handle = CreateEntityByHandle();
        GetEntity(handle).TrySetParent(parent);
        return handle;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<Entity> CreateEntityByHandle(Handle<Entity> parent, StringID name)
    {
        var handle = CreateEntityByHandle(name);
        GetEntity(handle).TrySetParent(parent);
        return handle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<Entity> CreateEntityByHandle(ref Entity parent)
    {
        var handle = CreateEntityByHandle();
        GetEntity(handle).TrySetParent(parent.Handle);
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
    internal Handle<T> CreateComponent<T>(Handle<Entity> entityHandle, T component) where T : struct, IComponent
    {
        MarkHierarchyDirty();
        return GetStore<T>().Add(component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool RemoveComponent<T>(Handle<Entity> entityHandle, Handle<T> component) where T : struct, IComponent
    {
        MarkHierarchyDirty();
        return GetStore<T>().TryRemove(component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T GetComponent<T>(Handle<T> componentHandle) where T : struct, IComponent => ref GetStore<T>().Get(componentHandle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool RemoveComponent<T>(Handle<T> componentHandle) where T : struct, IComponent
    {
        MarkHierarchyDirty();
        return GetStore<T>().TryRemove(componentHandle);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool RemoveComponent(int typeId, Handle<IComponent> iComponent)
    {
        MarkHierarchyDirty();
        return _componentStores[typeId].TryRemove(iComponent);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool RemoveComponent(int typeId, Handle iComponent)
    {
        MarkHierarchyDirty();
        return _componentStores[typeId].TryRemove(iComponent);
    }

    public void Update(GameTime gameTime)
    {
        if (UseHierarchyScheduler)
        {
            OrderHierachyIfDirty();

            // This should be different in this version but I don't have it yet
            foreach (var store in _componentStores)
                store?.InitializePendingComponents();

            foreach (var item in _updateNodes)
                _componentStores[item.type].UpdateComponent(item.handle, gameTime);
        }
        else
        {
            foreach (var store in _componentStores)
            {
                store?.InitializePendingComponents();
                store?.Update(gameTime);
            }
        }
    }

    public void FixedUpdate(GameTime gameTime)
    {
        if (UseHierarchyScheduler)
        {
            OrderHierachyIfDirty();
            foreach (var item in _fixedUpdateNodes)
                _componentStores[item.type].FixedUpdateComponent(item.handle, gameTime);
        }
        else
        {
            foreach (var store in _componentStores)
                store?.FixedUpdate(gameTime);
        }
    }

    public void Draw(GameTime gameTime)
    {
        if (UseHierarchyScheduler)
        {
            OrderHierachyIfDirty();
            foreach (var item in _drawNodes)
                _componentStores[item.type].DrawComponent(item.handle, gameTime);
        }
        else
        {
            foreach (var store in _componentStores)
                store?.Draw(gameTime);
        }
    }

    IEnumerator<HandleItem<Entity>> IEnumerable<HandleItem<Entity>>.GetEnumerator() => GetEnumerator();
    IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public HandleMapGrowing<Entity>.Enumerator GetEnumerator() => _entities.GetEnumerator();

    internal delegate void EntityVisitor(ref Entity entity);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void TraverseHierarchy(Handle<Entity> root, EntityVisitor action)
    {
        var current = root;

        while (current.Id != 0)
        {
            ref var entity = ref GetEntity(current);

            action(ref entity);

            if (entity._firstChild.Id != 0)
            {
                current = entity._firstChild;
                continue;
            }

            while (current.Id != 0)
            {
                ref var currentEntity = ref GetEntity(current);

                if (currentEntity._nextSibling.Id != 0)
                {
                    current = currentEntity._nextSibling;
                    break;
                }

                current = currentEntity._parent;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void TraverseHierarchy(Handle<Entity> root, Action<Handle<Entity>> action)
    {
        var current = root;

        while (current.Id != 0)
        {
            ref var entity = ref GetEntity(current);

            action(current);

            if (entity._firstChild.Id != 0)
            {
                current = entity._firstChild;
                continue;
            }

            while (current.Id != 0)
            {
                ref var currentEntity = ref GetEntity(current);

                if (currentEntity._nextSibling.Id != 0)
                {
                    current = currentEntity._nextSibling;
                    break;
                }

                current = currentEntity._parent;
            }
        }
    }

    public bool IsValid(Handle<Entity> entity) => _entities.IsValid(entity);

    public void EditorDraw()
    {
        ImGui.Begin("World");
        bool tmp = _useHierachyScheduler;
        if (ImGui.Checkbox("Use hierarchy", ref tmp))
            UseHierarchyScheduler = tmp;
        ImGui.End();

        if (_entities.Count > 50) 
            return;

        ImGui.Begin("Entities");

        void DrawEntity(Handle<Entity> handle)
        {
            var style = ImGui.GetStyle();

            style.IndentSpacing = 16.0f;
            style.TreeLinesSize = 1.0f;
            style.TreeLinesRounding = 0.0f;

            ref Entity entity = ref GetEntity(handle);

            bool hasChildren = entity._firstChild.Id != 0;

            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.SpanAvailWidth | 
                                       ImGuiTreeNodeFlags.OpenOnArrow |
                                       ImGuiTreeNodeFlags.DrawLinesFull;

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
