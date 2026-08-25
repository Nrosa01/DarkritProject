// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.Base;
using Darkrit.DataStructures;
using Darkrit.Editor;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Darkrit.EntityModel;

/// <summary>
/// Interface for the container that holds component of a specified type
/// </summary>
public interface IComponentStore
{
    /// <summary>
    /// Name of the Component type stored in this store
    /// </summary>
    StringID Name { get; }

    /// <summary>
    /// Currently unused, initialized recently created componentes
    /// </summary>
    public void InitializePendingComponents();
    
    /// <summary>
    /// Calls every component <see cref="IComponent.Update(GameTime)"/> if they're enabled
    /// </summary>
    /// <param name="gameTime"></param>
    public void Update(GameTime gameTime);

    /// <summary>
    /// Calls every component <see cref="IComponent.FixedUpdate(GameTime)"/> if they're enabled
    /// </summary>
    /// <param name="gameTime"></param>
    public void FixedUpdate(GameTime gameTime);

    /// <summary>
    /// Calls every component <see cref="IComponent.LateUpdate(GameTime)"/> if they're enabled
    /// Runs after both <see cref="Update(GameTime)"/> and <see cref="FixedUpdate(GameTime)"/>
    /// </summary>
    /// <param name="gameTime"></param>
    void LateUpdate(GameTime gameTime);


    /// <summary>
    /// Calls every component <see cref="IComponent.Draw(GameTime)"/> if they're enabled
    /// </summary>
    /// <param name="gameTime"></param>
    public void Draw(GameTime gameTime);

    /// <summary>
    /// Removes a component given a handle.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns>True if the component was removed</returns>
    public bool TryRemove(Handle<IComponent> handle);
    
    /// <summary>
    /// Removes a component given an untyped handle.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns>True if the component was removed</returns>
    public bool TryRemove(Handle handle);

    /// <summary>
    /// Should not be called directly. This callback is for when
    /// an entity is enabled or disabled, to inform its components
    /// </summary>
    /// <param name="status"></param>
    /// <param name="handle"></param>
    public void EntityActiveInHierarchyChanged(bool status, Handle handle);

    /// <summary>
    /// Updates an specific component given an untyped handle
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="gameTime"></param>
    void UpdateComponent(Handle handle, GameTime gameTime);
    
    /// <summary>
    /// FixesUpdate a specific component given an untyped handle
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="gameTime"></param>
    public void FixedUpdateComponent(Handle handle, GameTime gameTime);
    
    /// <summary>
    /// Draws a specific component given an untyped handle
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="gameTime"></param>
    void DrawComponent(Handle handle, GameTime gameTime);
    
    /// <summary>
    /// LateUpdate a component given a untyped handle
    /// </summary>
    /// <param name="gameTime"></param>
    void LateUpdateComponent(Handle handle, GameTime gameTime);

    /// <summary>
    /// Whether the component is Updeable
    /// </summary>
    bool IsUpdateable { get; }

    /// <summary>
    /// Whether the component is FixedUpdateable
    /// </summary>
    bool IsFixedUpdateable { get; }

    /// <summary>
    /// Whether the component is Drawable
    /// </summary>
    bool IsDrawable { get; }

    /// <summary>
    /// Priority of the component execution. Lower values means more priority
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Draws the IMGUI window for the given component
    /// </summary>
    /// <param name="handle"></param>
    void EditorDraw(Handle handle);
}

/// <summary>
/// I cache here the activeInHierachy from the parent, because calling the
/// entity through the component handle incurs in an indirection that for some
/// reason is costly, even tho the damn getter is used inside the entity
/// 
/// Doing this cache is much faster in my benchmarks, so I will go with it
/// </summary>
internal struct ComponentMetadata
{
    internal bool _activeInHierarchy = false;
    internal bool _initialized = false;

    public readonly bool CanExecute => _activeInHierarchy && _initialized;

    public ComponentMetadata()
    {
    }
}


/// <summary>
/// Storage for components of a specific type
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="initialCapacity"></param>
public class ComponentStore<T>(int initialCapacity) : IComponentStore, IEnumerable<T> where T : struct, IComponent, IHandle<T>
{
    private static readonly bool IsUpdateable = typeof(T).IsDefined(typeof(UpdateableAttribute), inherit: false);
    
    private static readonly bool IsLateUpdateable = typeof(T).IsDefined(typeof(LateUpdateableAttribute), inherit: false);

    private static readonly bool IsFixedUpdateable = typeof(T).IsDefined(typeof(FixedUpdateableAttribute), inherit: false);

    private static readonly bool IsDrawable = typeof(T).IsDefined(typeof(DrawableAttribute), inherit: false);
    
    private static readonly bool OverridesPriority = typeof(T).IsDefined(typeof(PriorityAttribute), inherit: false);
    
    internal static readonly int Priority = OverridesPriority ? typeof(T).GetCustomAttribute<PriorityAttribute>(inherit: false).Priority : 0;

    /// <inheritdoc/>
    public static readonly StringID NameID = new(typeof(T).Name);

    /// <inheritdoc/>
    StringID IComponentStore.Name => NameID;

    /// <inheritdoc/>
    bool IComponentStore.IsUpdateable => IsUpdateable;

    /// <inheritdoc/>
    bool IComponentStore.IsFixedUpdateable => IsFixedUpdateable;

    /// <inheritdoc/>
    bool IComponentStore.IsDrawable => IsDrawable;

    private readonly HandleMapGrowing<T> _components = new(initialCapacity);
    readonly GrowableArray<ComponentMetadata> _componentMetadata = new(initialCapacity);

    private readonly Stack<Handle<T>> nonInitializedComponents = new();

    /// <summary>
    /// Amount of components in use
    /// </summary>
    public int Count => _components.Count;

    /// <inheritdoc/>
    int IComponentStore.Priority => Priority;

    
    /// <inheritdoc/>
    public void InitializePendingComponents()
    {
        while (nonInitializedComponents.TryPop(out Handle<T> handle))
        {
            ref var component = ref _components.At(handle.Id);
            _componentMetadata[handle.Id]._activeInHierarchy = _components.At(handle.Id).ActiveInHierachy;
            _componentMetadata[handle.Id]._initialized = true;
            
            if (component.Enabled && _componentMetadata[handle.Id].CanExecute)
                component.OnEnable();
            
            component.OnAdd();
            component.World.GetEntity(component.EntityHandle).ResetInterpolation();
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Add(T value)
    {
        var handle = _components.Add(value);

        nonInitializedComponents.Push(handle);

        if (handle.Id < _componentMetadata.Count)
            _componentMetadata[handle.Id] = default;
        else
            _componentMetadata.Add(default);

        ref var component = ref Get(handle);
        component.OnCreate();
        return ref component;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get(Handle<T> componentHandle) => ref _components[componentHandle];

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Handle<T> componentHandle) => _components.IsValid(componentHandle);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRemove(Handle<T> componentHandle)
    {
        _components.At(componentHandle.Id).OnDisable();
        _components.At(componentHandle.Id).OnRemove();
        return _components.Remove(componentHandle);
    }

    /// <inheritdoc/>
    public void Update(GameTime gameTime)
    {
        InitializePendingComponents();
        
        if (!IsUpdateable) return;

        foreach (ref var handleItem in _components)
        {
            if (handleItem.Enabled && _componentMetadata[handleItem.Handle.Id].CanExecute)
                handleItem.Update(gameTime);
        }
    }

    /// <inheritdoc/>
    public void LateUpdate(GameTime gameTime)
    {
        if (!IsLateUpdateable) return;

        foreach (ref var handleItem in _components)
        {
            if (handleItem.Enabled && _componentMetadata[handleItem.Handle.Id].CanExecute)
                handleItem.LateUpdate(gameTime);
        }
    }

    /// <inheritdoc/>
    public void FixedUpdate(GameTime gameTime)
    {
        if (!IsFixedUpdateable) return;

        foreach (ref var handleItem in _components)
        {
            if (handleItem.Enabled && _componentMetadata[handleItem.Handle.Id].CanExecute)
                handleItem.FixedUpdate(gameTime);
        }
    }

    /// <inheritdoc/>
    public void Draw(GameTime gameTime)
    {
        if (!IsDrawable) return;

        foreach (ref var handleItem in _components)
        {
            if (handleItem.Enabled && _componentMetadata[handleItem.Handle.Id].CanExecute)
                handleItem.Draw(gameTime);
        }
    }

    /// <inheritdoc/>
    public bool TryRemove(Handle<IComponent> handle)
    {
        return TryRemove(new Handle<T>
        {
            Id = handle.Id,
            Generation = handle.Generation
        });
    }

    /// <inheritdoc/>
    public bool TryRemove(Handle handle)
    {
        return TryRemove(new Handle<T>
        {
            Id = handle.Id,
            Generation = handle.Generation
        });
    }

    /// <inheritdoc/>
    public void EntityActiveInHierarchyChanged(bool status, Handle handle)
    {
        ref var component = ref _components.At(handle.Id);

        _componentMetadata[handle.Id]._activeInHierarchy = status;

        if (!component.Enabled) return;

        if (component.Enabled)
        {
            if (status)
                component.OnEnable();
            else
                component.OnDisable();
        }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    /// <inheritdoc/>
    public HandleMapGrowing<T>.Enumerator GetEnumerator() => _components.GetEnumerator();

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateComponent(Handle handle, GameTime gameTime) => _components.At(handle.Id).Update(gameTime);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FixedUpdateComponent(Handle handle, GameTime gameTime) => _components.At(handle.Id).FixedUpdate(gameTime);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawComponent(Handle handle, GameTime gameTime) => _components.At(handle.Id).Draw(gameTime);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LateUpdateComponent(Handle handle, GameTime gameTime) => _components.At(handle.Id).LateUpdate(gameTime);

    // Fields of the component that I will display, this includes auto properties by default
    // I exclude the "Enabled" property as I handle that manually in the collapsing header
    private static readonly FieldInfo[] EditorFields = [.. typeof(T)
    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
    .Where(field =>
        !field.IsStatic &&
        EditorFieldDrawer.GetFieldName(field) != nameof(IComponent.Enabled) &&
        (field.IsPublic ||
         field.IsDefined(typeof(ShowInInspectorAttribute)) ||
         field.IsDefined(typeof(SerializeFieldAttribute)) ||
         field.Name.Contains("k__BackingField")))];

    private readonly HashSet<int> _editorExpanded = [];

    /// <inheritdoc/>
    public void EditorDraw(Handle handle)
    {
        ImGui.PushID((int)NameID.ID);
        ImGui.PushID(handle.Id);

        ref T component = ref _components.At(handle.Id);
        var wasEnabled = component.Enabled;

        if (!wasEnabled)
            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1f));

        int editorFieldCount = EditorFieldDrawer.GetEditorFieldCount(EditorFields);
        bool hasFields = editorFieldCount > 0;
        bool open = hasFields && _editorExpanded.Contains(handle.Id);

        // Draw the header background.
        System.Numerics.Vector2 headerMin = ImGui.GetCursorScreenPos();
        float headerHeight = ImGui.GetFrameHeight();
        float headerWidth = ImGui.GetContentRegionAvail().X;

        ImGui.GetWindowDrawList().AddRectFilled(
            headerMin,
            headerMin + new System.Numerics.Vector2(headerWidth, headerHeight),
            ImGui.GetColorU32(ImGuiCol.Header),
            16.0f);

        // Expand/collapse button.
        if (hasFields)
        {
            if (ImGui.ArrowButton("##Expand", open ? ImGuiDir.Down : ImGuiDir.Right))
            {
                open = !open;

                if (open)
                    _editorExpanded.Add(handle.Id);
                else
                    _editorExpanded.Remove(handle.Id);
            }
        }
        else
        {
            ImGui.BeginDisabled();
            ImGui.ArrowButton("##Expand", ImGuiDir.Right);
            ImGui.EndDisabled();
        }

        ImGui.SameLine();

        bool enabled = component.Enabled;
        if (ImGui.Checkbox("##Enabled", ref enabled))
            component.Enabled = enabled;

        ImGui.SameLine(0.0f, 6.0f);

        ImGui.Text(NameID.ToString());

        // Component fields.
        if (open)
        {
            if (ImGui.BeginTable("##Fields", 2))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 120.0f);
                ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);

                foreach (FieldInfo field in EditorFields)
                {
                    if (EditorFieldDrawer.IsEditorFieldSupported(field))
                        EditorFieldDrawer.Draw(field, ref component);
                }

                ImGui.EndTable();
            }
        }

        if (!wasEnabled)
            ImGui.PopStyleColor();

        ImGui.PopID();
        ImGui.PopID();
    }
}
