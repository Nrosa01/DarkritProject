// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Darkrit.Base;
using Darkrit.DataStructures;
using Darkrit.DevTools.Logger;
using Darkrit.Editor;
using Darkrit.ImGuiUtils;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;

namespace Darkrit.EntityModel;

/// <summary>
/// Static class to assign a unique ID to each component
/// </summary>
public static class ComponentTypeId
{
    private static int _nextId;

    /// <summary>
    /// Gets a new id
    /// </summary>
    /// <returns></returns>
    public static int Next() => Interlocked.Increment(ref _nextId) - 1;

    /// <summary>
    /// Total amount of IComponent types
    /// </summary>
    public static readonly int Count = ReflectionUtils.CountDerivedTypes<IComponent>();
}

/// <summary>
/// Generic version that each type uses to generate its IDs
/// </summary>
/// <typeparam name="T"></typeparam>
public static class ComponentTypeId<T> where T : struct, IComponent
{
    /// <summary>
    /// ID of this component
    /// </summary>
    public static readonly int Id = ComponentTypeId.Next();
}

internal struct EntityMetadata
{
    public Handle<Entity> _parent;
    public Handle<Entity> _firstChild;
    public Handle<Entity> _lastChild;
    public Handle<Entity> _nextSibling;
    public Handle<Entity> _previousSibling;
    public int _childCount;
}

/// <summary>
/// Class tha own entities and components and orchestrates them
/// </summary>
/// <param name="initialCapacity"></param>
public class EntityRegistry(int initialCapacity) : IEnumerable<Entity>
{
    private readonly IComponentStore[] _componentStores = new IComponentStore[ComponentTypeId.Count];
    private readonly int[] _componentStoresOrder = new int[ComponentTypeId.Count];
    private readonly HandleMapGrowing<Entity> _entities = new(initialCapacity);
    private readonly GrowableArray<ComponentList> _entityComponents = [new()];
    private readonly GrowableArray<EntityMetadata> _entityMetadata = [new()];

    private readonly GrowableArray<TypedHandle> _updateNodes = [];
    private readonly GrowableArray<TypedHandle> _fixedUpdateNodes = [];
    private readonly GrowableArray<TypedHandle> _drawNodes = [];

    // Not all component stores are in use always, this number represents the number of active component stores
    // This is because I initialize _componentStores to the number of Component types, but the store isn't created
    // until the component is in need. Due to reflection limits I initialize stores on demand in the generic function
    private int _componentStoresCount;

    // Internal getters so the entity has access to rarely used properties-
    // Making the struct bigger, makes the simulation slower when there are many entities, so I add a bit of indirection
    // Saving these data separately from the Entity struct. Profiling proved this to work
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref EntityMetadata MetadataOf(Handle<Entity> handle) => ref _entityMetadata[handle.Id];
    
    // I don't really need to do that for the ComponentList given it's just the size of a IntPtr32
    // But this allows to use component lists when creating/destroying entities
    // This is not the best way to do it, but for now it works well enough
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref ComponentList ComponentsOf(Handle<Entity> handle) => ref _entityComponents[handle.Id];

    private void AddStoreOrder(int id)
    {
        int priority = _componentStores[id].Priority;
        int i = _componentStoresCount;

        while (i > 0)
        {
            int previousId = _componentStoresOrder[i - 1];

            if (_componentStores[previousId].Priority <= priority)
                break;

            _componentStoresOrder[i] = previousId;
            i--;
        }

        _componentStoresOrder[i] = id;
        _componentStoresCount++;
    }

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
            if (!item.HasParent)
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

    /// <summary>
    /// Amount of active entities in the world
    /// </summary>
    public int Count => _entities.Count;

    /// <summary>
    /// Amount of times <see cref="FixedUpdate(GameTime)"/> has executed since the beggining
    /// </summary>
    public ulong FixedTick { get; internal set; }

    /// <summary>
    /// Amount of times <see cref="Update(GameTime)"/> has executed since the beggining
    /// </summary>
    public ulong Tick { get; internal set; }

    /// <summary>
    /// Amount of times <see cref="Draw(GameTime)"/> has executed since the beggining
    /// </summary>
    public ulong RenderFrame { get; internal set; }

    /// <summary>
    /// This value stores how far we are in the current frame. For example, when the 
    /// value of ALPHA is 0.5, it means we are halfway between the last frame and the 
    /// next upcoming frame
    /// </summary>
    public static float FixedUpdateAlpha => Core.FixedUpdateAlpha;

    /// <summary>
    /// Gets a entity given a handle. If it doesn't exist it gets a default one
    /// </summary>
    /// <param name="entityHandle"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Entity GetEntity(Handle<Entity> entityHandle) => ref _entities[entityHandle];


    /// Gets a entity given a handle. If it doesn't exist it gets a default one
    /// </summary>
    /// <param name="entityHandle"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref readonly Entity GetEntityReadonly(Handle<Entity> entityHandle) => ref _entities.GetReadonly(entityHandle);

    /// <summary>
    /// Creates an entity registry with initial capacity of 1000 entities
    /// </summary>
    public EntityRegistry() : this(1000) { }


    /// <summary>
    /// Gets a component of store of type T
    /// It creates the store if it didn't exist
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ComponentStore<T> GetStore<T>() where T : struct, IComponent, IHandle<T>
    {
        int id = ComponentTypeId<T>.Id;

        if (_componentStores[id] is not ComponentStore<T> store) // Happens when is null
        {
            store = new ComponentStore<T>(initialCapacity);
            _componentStores[id] = store;
            AddStoreOrder(id);
        }

        return store;
    }

    /// <summary>
    /// Creates an entity and returns a handle to it
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<Entity> CreateEntityByHandle(string name = "") => CreateEntityByHandle(new StringID(name));

    /// <summary>
    /// Creates an entity and returns a handle to it
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<Entity> CreateEntityByHandle(StringID name)
    {
        var handle = _entities.PeekNextHandle();

        if (handle.Id < _entityMetadata.Count)
        {
            _entityMetadata[handle.Id] = default;
            _entityComponents[handle.Id].Clear();
        }
        else
        {
            _entityMetadata.Add(default);
            _entityComponents.Add(new());
        }

        _entities.Add(new Entity
        {
            NameID = name,
            World = this,
            Handle = _entities.PeekNextHandle(),
            ActiveSelf = true,
            ActiveInHierarchy = true,
        });

        return handle;
    }

    /// <summary>
    /// Creates an entity and returns a reference to it
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Entity CreateEntity(string name = "")
    {
        Handle<Entity> handle = CreateEntityByHandle(new StringID(name));
        return ref GetEntity(handle);
    }

    /// <summary>
    /// Creates an entity and returns a reference to it
    /// The created entity will be a child of <paramref name="parentHandle"/>
    /// </summary>
    /// <param name="parentHandle"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Entity CreateEntity(Handle<Entity> parentHandle, string name = "")
    {
        ref Entity entity = ref CreateEntity(name);
        entity.TrySetParent(parentHandle);
        return ref entity;
    }

    /// <summary>
    /// Creates an entity and returns a reference to it
    /// The created entity will be a child of <paramref name="parent"/>
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Entity CreateEntity(ref Entity parent, string name = "")
    {
        ref Entity entity = ref CreateEntity(name);
        entity.TrySetParent(parent.Handle);
        return ref entity;
    }


    /// <summary>
    /// Creates an entity and returns a handle to it
    /// The created entity will be a child of <paramref name="parentHandle"/>
    /// </summary>
    /// <param name="parentHandle"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<Entity> CreateEntityByHandle(Handle<Entity> parentHandle, string name = "")
    {
        var handle = CreateEntityByHandle(new StringID(name));
        GetEntity(handle).TrySetParent(parentHandle);
        return handle;
    }


    /// <summary>
    /// Creates an entity and returns a handle to it
    /// The created entity will be a child of <paramref name="parent"/>
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<Entity> CreateEntityByHandle(ref Entity parent, string name = "")
    {
        var handle = CreateEntityByHandle(new StringID(name));
        GetEntity(handle).TrySetParent(parent.Handle);
        return handle;
    }

    /// <summary>
    /// Removes an entity by its handle
    /// </summary>
    /// <param name="handle"></param>
    /// <returns>True if the entity was removed</returns>
    public bool TryRemoveEntity(Handle<Entity> handle)
    {
        if (!_entities.IsValid(handle))
            return false;

        var child = _entities[handle]._firstChild;

        while (child.Id != 0)
        {
            var next = _entities[child]._nextSibling;

            TryRemoveEntity(child);

            child = next;
        }

        _entities[handle].Release();

        return _entities.Remove(handle);
    }

    /// <summary>
    /// Checks if a handle to an entity is valid
    /// </summary>
    /// <param name="entityHandle"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsValid(Handle<Entity> entityHandle) => _entities.IsValid(entityHandle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T CreateComponent<T>(Handle<Entity> entityHandle, T component) where T : struct, IComponent, IHandle<T>
    {
        MarkHierarchyDirty();
        return ref GetStore<T>().Add(component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool RemoveComponent<T>(Handle<Entity> entityHandle, Handle<T> component) where T : struct, IComponent, IHandle<T>
    {
        MarkHierarchyDirty();
        return GetStore<T>().TryRemove(component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T GetComponent<T>(Handle<T> componentHandle) where T : struct, IComponent, IHandle<T> => ref GetStore<T>().Get(componentHandle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool RemoveComponent<T>(Handle<T> componentHandle) where T : struct, IComponent, IHandle<T>
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

    internal void EntityActiveInHierarchyChanged(bool entityEnabled, int type, Handle handle) => _componentStores[type].EntityActiveInHierarchyChanged(entityEnabled, handle);

    /// <summary>
    /// Updates all of the components
    /// </summary>
    /// <param name="gameTime"></param>
    public void Update(GameTime gameTime)
    {
        Tick++;
        if (UseHierarchyScheduler)
        {
            OrderHierachyIfDirty();

            for (int i = 0; i < _componentStoresCount; i++)
            {
                var store = _componentStores[_componentStoresOrder[i]];
                store.InitializePendingComponents();
            }

            foreach (var item in _updateNodes)
                _componentStores[item.type].UpdateComponent(item.handle, gameTime);
        }
        else
        {
            for (int i = 0; i < _componentStoresCount; i++)
            {
                var store = _componentStores[_componentStoresOrder[i]];
                store.Update(gameTime);
            }
        }
    }

    /// <summary>
    /// Late Update all components
    /// </summary>
    /// <param name="gameTime"></param>
    public void LateUpdate(GameTime gameTime)
    {
        if (UseHierarchyScheduler)
        {
            OrderHierachyIfDirty();

            foreach (var item in _updateNodes)
                _componentStores[item.type].LateUpdateComponent(item.handle, gameTime);
        }
        else
        {
            for (int i = 0; i < _componentStoresCount; i++)
            {
                var store = _componentStores[_componentStoresOrder[i]];
                store.LateUpdate(gameTime);
            }
        }
    }

    /// <summary>
    /// Fixed Update all components
    /// </summary>
    /// <param name="gameTime"></param>
    public void FixedUpdate(GameTime gameTime)
    {
        FixedTick++;

        if (UseHierarchyScheduler)
        {
            OrderHierachyIfDirty();
            foreach (var item in _fixedUpdateNodes)
                _componentStores[item.type].FixedUpdateComponent(item.handle, gameTime);
        }
        else
        {
            for (int i = 0; i < _componentStoresCount; i++)
            {
                var store = _componentStores[_componentStoresOrder[i]];
                store.FixedUpdate(gameTime);
            }
        }
    }

    /// <summary>
    /// True when the <see cref="EntityRegistry"/> is in the middle of the <see cref="Draw(GameTime)"/> callback
    /// </summary>
    public bool IsDrawing { get; private set; }

    /// <summary>
    /// Calls Draw on all components
    /// </summary>
    /// <param name="gameTime"></param>
    public void Draw(GameTime gameTime)
    {
        RenderFrame++;
        IsDrawing = true;

        if (UseHierarchyScheduler)
        {
            OrderHierachyIfDirty();
            foreach (var item in _drawNodes)
                _componentStores[item.type].DrawComponent(item.handle, gameTime);
        }
        else
        {
            for (int i = 0; i < _componentStoresCount; i++)
            {
                var store = _componentStores[_componentStoresOrder[i]];
                store.Draw(gameTime);
            }
        }

        IsDrawing = false;
    }

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

    private readonly List<(Handle<Entity> Handle, int Depth)> _editorVisibleEntities = [];
    private readonly HashSet<int> _editorCollapsedEntities = [];
    private readonly Dictionary<int, StringID> _editorFallbackNames = [];

    /// <summary>
    /// Does the debug render, still WIP
    /// </summary>
    public void EditorDraw()
    {
        ImGui.Begin("World");

        bool tmp = _useHierachyScheduler;
        if (ImGui.Checkbox("Use hierarchy", ref tmp))
            UseHierarchyScheduler = tmp;

        ImGui.End();

        ImGui.Begin("Entities");

        var style = ImGui.GetStyle();
        style.IndentSpacing = 16.0f;
        style.TreeLinesSize = 1.0f;
        style.TreeLinesRounding = 0.0f;

        _editorVisibleEntities.Clear();

        // Build a flat list of visible entities.
        foreach (ref Entity root in this)
        {
            if (root._parent.Id != 0)
                continue;

            var current = root.Handle;
            int depth = 0;

            while (current.Id != 0)
            {
                ref Entity entity = ref GetEntity(current);

                _editorVisibleEntities.Add((current, depth));

                bool hasChildren = entity._firstChild.Id != 0;
                bool isCollapsed = _editorCollapsedEntities.Contains(current.Id);

                if (hasChildren && !isCollapsed)
                {
                    current = entity._firstChild;
                    depth++;
                    continue;
                }

                while (current.Id != 0)
                {
                    ref Entity currentEntity = ref GetEntity(current);

                    if (currentEntity._nextSibling.Id != 0)
                    {
                        current = currentEntity._nextSibling;
                        break;
                    }

                    current = currentEntity._parent;
                    depth--;
                }
            }
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(_editorVisibleEntities.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var row = _editorVisibleEntities[i];

                ref Entity entity = ref GetEntity(row.Handle);

                bool hasChildren = entity._firstChild.Id != 0;
                bool isOpen = !_editorCollapsedEntities.Contains(row.Handle.Id);

                ImGuiTreeNodeFlags flags =
                    ImGuiTreeNodeFlags.SpanAvailWidth |
                    ImGuiTreeNodeFlags.OpenOnArrow |
                    ImGuiTreeNodeFlags.DrawLinesFull |
                    ImGuiTreeNodeFlags.NoTreePushOnOpen;

                if (!hasChildren)
                    flags |= ImGuiTreeNodeFlags.Leaf;

                if (hasChildren)
                    ImGui.SetNextItemOpen(isOpen);

                if (row.Depth > 0)
                    ImGui.Indent(row.Depth * style.IndentSpacing);

                if (!entity.ActiveInHierarchy)
                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1f));

                StringID name = entity.NameID;

                // Names take space in memory and put pressure in GC
                // The editor should NEVER modify entities to work properly, so I create
                // names for unnamed entities in a cache just to display something to screen
                if (!name.IsValid)
                {
                    if (!_editorFallbackNames.TryGetValue(row.Handle.Id, out name))
                    {
                        name = new StringID($"Entity {row.Handle.Id}");
                        _editorFallbackNames.Add(row.Handle.Id, name);
                    }
                }


                // Handle.Id is the ImGui identity; Name is only the visible label
                // This way I can safely rename the entity
                ImGui.PushID(row.Handle.Id);

                ImGui.TreeNodeEx(name.ToString(), flags);

                if (!entity.ActiveInHierarchy)
                    ImGui.PopStyleColor();

                if (hasChildren && ImGui.IsItemToggledOpen())
                {
                    if (isOpen)
                        _editorCollapsedEntities.Add(row.Handle.Id);
                    else
                        _editorCollapsedEntities.Remove(row.Handle.Id);
                }

                if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                {
                    _inspectedEntity = row.Handle;
                    _inspectorOpen = true;
                }

                ImGui.PopID();

                if (row.Depth > 0)
                    ImGui.Unindent(row.Depth * style.IndentSpacing);
            }
        }

        clipper.End();

        DrawInspector();

        ImGui.End();
    }

    // The fields of the entity that I will be displaying
    // I don't really need this but right now I'm based all my Editor system on reflected data, so providing FieldInfo
    // Makes everything easier for me until I make something better
    private static readonly FieldInfo TransformField =typeof(Entity).GetField("_current", BindingFlags.Instance | BindingFlags.NonPublic);

    private Handle<Entity> _inspectedEntity;
    bool _inspectorOpen = true;
    private void DrawInspector()
    {
        if (!_inspectorOpen || _inspectedEntity.Id == 0 || !IsValid(_inspectedEntity))
            return;

        ref Entity entity = ref GetEntity(_inspectedEntity);

        if (!ImGui.Begin("Inspector", ref _inspectorOpen))
        {
            ImGui.End();
            return;
        }

        StringID name = entity.NameID;

        if (!name.IsValid)
        {
            if (!_editorFallbackNames.TryGetValue(_inspectedEntity.Id, out name))
            {
                name = new StringID($"Entity {_inspectedEntity.Id}");
                _editorFallbackNames.Add(_inspectedEntity.Id, name);
            }
        }

        bool active = entity.ActiveSelf;
        if(ImGui.Checkbox("##Active", ref active))
            entity.ActiveSelf = active;

        ImGui.SameLine(0.0f, 6.0f);
        ImGui.Text(name.ToString());

        ImGui.Separator();

        // Transform is a bit special as it's manually "inlined"
        // Instead of being property => value
        // It's just value
        // Idk how to explain it but I hope I remember what this means when I have to refactor this
        ImGui.PushID("Transform");

        if (ImGui.CollapsingHeader("Transform", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (ImGui.BeginTable("##TransformFields", 2))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 120.0f);
                ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);

                if (EditorFieldDrawer.Draw(TransformField, ref entity, false))
                    entity.ResetInterpolation();

                ImGui.EndTable();
            }
        }

        ImGizmo2D.SetHandleRadius(8.0f);

        // This is for testing gizmos, it will be removed from here or changed in the future
        var position = entity.Position;
        var rotation = entity.RotationDegrees;
        var scale = entity.Scale;

        if (ImGizmo2D.Translate("EntityTranslate", ref position.X, ref position.Y))
            entity.Position = position;

        //if (ImGizmo2D.Rotate("EntityRotate", position.X, position.Y, ref rotation))
        //    entity.RotationDegrees = rotation;

        //if (ImGizmo2D.Scale("EntityScale", position.X, position.Y, ref scale.X, ref scale.Y))
        //    entity.Scale = scale;


        ImGui.PopID();

        ImGui.Separator();

        foreach (TypedHandle component in entity.Components)
        {
            IComponentStore store = _componentStores[component.type];
            store.EditorDraw(component.handle);
        }

        ImGui.End();
    }
}
