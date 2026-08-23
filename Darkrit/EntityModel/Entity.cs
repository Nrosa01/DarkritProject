// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Darkrit.Base;
using Darkrit.DevTools.Logger;
using Darkrit.Math;
using Microsoft.Xna.Framework;
using Handle = Darkrit.Base.Handle;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Darkrit.EntityModel;

struct ComponentList
{
    List<TypedHandle> _handles = [];

    public readonly IReadOnlyList<TypedHandle> Components => _handles;
    public ComponentList()
    {
        _handles = [];
    }

    public readonly void Add<T>(Handle<T> handle) where T : struct, IComponent => _handles.Add(TypedHandle.Create(handle));

    public readonly Handle<T> Get<T>() where T : struct, IComponent
    {
        int id = ComponentTypeId<T>.Id;
        foreach (var item in _handles)
        {
            if (item.type == id)
                return new Handle<T>
                {
                    Id = item.handle.Id,
                    Generation = item.handle.Generation
                };
        }

        return Handle<T>.Default;
    }

    public readonly Handle Remove<T>() where T : struct, IComponent => Remove<T>(default, true);

    public readonly Handle Remove<T>(Handle<T> handle) where T : struct, IComponent => Remove<T>(handle, false);

    private readonly Handle Remove<T>(Handle<T> handle, bool onlyCheckType) where T : struct, IComponent
    {
        int id = ComponentTypeId<T>.Id;

        int toRemove = -1;

        for (int i = 0; i < _handles.Count; i++)
        {
            TypedHandle item = _handles[i];
            if (item.type == id && (onlyCheckType || (item.handle.Id == handle.Id && item.handle.Generation == handle.Generation)))
            {
                toRemove = i;
                break;
            }
        }

        // Swap and remove
        if (toRemove != -1)
        {
            var handleToRemove = _handles[toRemove];
            _handles[toRemove] = _handles[_handles.Count - 1];
            _handles.RemoveAt(_handles.Count - 1);

            return handleToRemove.handle;
        }

        return Handle.Default;
    }

    internal readonly void Clear() => _handles.Clear();

    internal readonly bool Has<T>(Handle<T> componentHandle) where T : struct, IComponent
    {
        var typed = TypedHandle.Create(componentHandle);
        return _handles.Contains(typed);
    }
}

/// <summary>
/// Fundamental unit of the entity model
/// Entities contain handles to componentes, as well as to other entities
/// An entity is an intrusive list that creates an acyclic undirected graph
/// </summary>
public struct Entity : IHandle<Entity>
{
    /// <summary>
    /// Flags to use as needed, ideally assigned from an enum
    /// </summary>
    public int Flags;

    /// <summary>
    /// Name of this entity, stored as a single int
    /// </summary>
    public StringID NameID { readonly get; internal set; }

    /// <summary>
    /// Gets/Sets the name of the Entity
    /// </summary>
    public string Name
    {
        readonly get => NameID.ToString();
        set => NameID = new(value);
    }

    /// <summary>
    /// <see cref="EntityRegistry"/> this <see cref="Entity"/> belongs to
    /// </summary>
    public EntityRegistry World { get; init; }

    readonly ComponentList _componentList = new();

    /// <summary>
    /// Readonly list of the components this entity has
    /// </summary>
    public readonly IReadOnlyList<TypedHandle> Components => _componentList.Components;

    /// <inheritdoc/>
    public Handle<Entity> Handle { get; set; }

    internal Handle<Entity> _parent;
    internal Handle<Entity> _firstChild;
    internal Handle<Entity> _lastChild;
    internal Handle<Entity> _nextSibling;
    internal Handle<Entity> _previousSibling;
    private int _childCount;

    /// <summary>
    /// Reference fo the parent entity. If the entity doesn't have
    /// a parent this returns an Invalid entity
    /// </summary>
    public readonly ref Entity Parent => ref World.GetEntity(_parent);

    /// <summary>
    /// Whether this entity has parent or not
    /// </summary>
    public readonly bool HasParent => _parent.Id != 0;

    /// <summary>
    /// Number of children this entity has
    /// </summary>
    public readonly int ChildCount => _childCount;


    /// <summary>
    /// Sets a new parent to this entity
    /// Returns false if the parent is the same or if it would create a cycle
    /// Let's say you have Entity A, B and C
    /// If you make B chil of A, you can't make A child of A
    /// If you make b child of A, and C child of B, you can't make A child of C
    /// 
    /// The entity will be the first one in the children list of the new parent
    /// </summary>
    /// <param name="newParent"></param>
    /// <returns>Returns false if the parent is the same or if it would create a cycle</returns>
    public bool TrySetParentFirst(Handle<Entity> newParent)
    {
        if (_parent == newParent || WouldCreateCycle(newParent))
            return false;

        UnlinkFromParent();

        if (newParent.Id == 0)
        {
            ActiveInHierarchy = ActiveSelf;
            return false;
        }

        ref Entity parent = ref World.GetEntity(newParent);

        _parent = newParent;
        _previousSibling = default;
        _nextSibling = parent._firstChild;

        if (parent._firstChild.Id != 0)
            World.GetEntity(parent._firstChild)._previousSibling = Handle;
        else
            parent._lastChild = Handle;

        parent._firstChild = Handle;
        parent._childCount++;

        World.MarkHierarchyDirty();
        UpdateActiveInHierarchy();
        return true;
    }

    /// <summary>
    /// Sets a new parent to this entity
    /// Returns false if the parent is the same or if it would create a cycle
    /// Let's say you have Entity A, B and C
    /// If you make B chil of A, you can't make A child of A
    /// If you make b child of A, and C child of B, you can't make A child of C
    /// 
    /// The entity will be the last one in the children list of the new parent
    /// </summary>
    /// <param name="newParent"></param>
    /// <returns>Returns false if the parent is the same or if it would create a cycle</returns>
    public bool TrySetParent(Handle<Entity> newParent)
    {
        if(_parent == newParent || WouldCreateCycle(newParent))
            return false;

        UnlinkFromParent();

        if (newParent.Id == 0)
        {
            ActiveInHierarchy = ActiveSelf;
            return false;
        }

        _parent = newParent;

        ref Entity parent = ref World.GetEntity(newParent);

        // Insertar al final de los hijos.
        if (parent._firstChild.Id == 0)
        {
            parent._firstChild = Handle;
            parent._lastChild = Handle;
            _previousSibling = default;
            _nextSibling = default;
        }
        else
        {
            var lastChild = parent._lastChild;
            ref Entity last = ref World.GetEntity(lastChild);

            last._nextSibling = Handle;
            _previousSibling = lastChild;
            _nextSibling = default;
            parent._lastChild = Handle;
        }

        parent._childCount++;

        World.MarkHierarchyDirty();
        UpdateActiveInHierarchy();
        return true;
    }


    /// <summary>
    /// Check to avoid cyclic graph when parenting
    /// It just checks that the new parent is not any 
    /// of the parents of this entity in the hiearchy
    /// </summary>
    /// <param name="newParent"></param>
    /// <returns></returns>
    private readonly bool WouldCreateCycle(Handle<Entity> newParent)
    {
        if (newParent.Id == 0)
            return false;

        if (newParent == Handle)
            return true;

        var current = newParent;

        while (current.Id != 0)
        {
            if (current == Handle)
                return true;

            current = World.GetEntity(current)._parent;
        }

        return false;
    }

    /// <summary>
    /// Adds an entity as a child of this one
    /// Under the hood is just calls the set parent of the child to set this entity as its parent
    /// </summary>
    /// <param name="child"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryAddChild(Handle<Entity> child) => World.GetEntity(child).TrySetParent(Handle);

    /// <summary>
    /// Sets the current entity as the sibling <paramref name="index"/> of its parent
    /// </summary>
    /// <param name="index"></param>
    /// <returns>
    /// False when:
    /// - Entity had no parent
    /// - Index was negative
    /// - Index was higher than parent <see cref="ChildCount"/>
    /// - Index was the same the entity was already at
    /// True when:
    /// - None of the previous happen and the operation modified the entity`s order in the parent
    /// </returns>
    public bool TrySetSiblingIndex(int index)
    {
        if (_parent.Id == 0 || index < 0)
            return false;

        ref Entity parent = ref World.GetEntity(_parent);

        if (index >= parent._childCount)
            return false;

        // Get current index.
        int currentIndex = 0;
        var child = parent._firstChild;

        while (child != Handle)
        {
            currentIndex++;
            child = World.GetEntity(child)._nextSibling;
        }

        if (currentIndex == index)
            return true;

        // Remove from the list.
        UnlinkFromParent();

        // Restore child count after unlinking.
        parent._childCount++;

        // Insert as first child.
        if (index == 0)
        {
            _parent = parent.Handle;
            _previousSibling = default;
            _nextSibling = parent._firstChild;

            if (_nextSibling.Id != 0)
                World.GetEntity(_nextSibling)._previousSibling = Handle;
            else
                parent._lastChild = Handle;

            parent._firstChild = Handle;

            World.MarkHierarchyDirty();
            return true;
        }

        // Find the element that will be immediately before this one.
        var previous = parent._firstChild;

        for (int i = 1; i < index; i++)
            previous = World.GetEntity(previous)._nextSibling;

        ref Entity previousEntity = ref World.GetEntity(previous);
        var next = previousEntity._nextSibling;

        _parent = parent.Handle;
        _previousSibling = previous;
        _nextSibling = next;

        previousEntity._nextSibling = Handle;

        if (next.Id != 0)
            World.GetEntity(next)._previousSibling = Handle;
        else
            parent._lastChild = Handle;

        World.MarkHierarchyDirty();
        return true;
    }

    /// <summary>
    /// This is the private helper that unparents the entity
    /// It's used to aid in other operations, but not exposed because
    /// unparenting from everything needs to update the <see cref="ActiveInHierarchy"/>
    /// status, so that's why <see cref="UnParent"/> exists
    /// </summary>
    private void UnlinkFromParent()
    {
        if (_parent.Id == 0)
            return;

        ref Entity parent = ref World.GetEntity(_parent);

        if (_previousSibling.Id != 0)
            World.GetEntity(_previousSibling)._nextSibling = _nextSibling;
        else
            parent._firstChild = _nextSibling;

        if (_nextSibling.Id != 0)
            World.GetEntity(_nextSibling)._previousSibling = _previousSibling;
        else
            parent._lastChild = _previousSibling;

        parent._childCount--;

        _parent = default;
        _previousSibling = default;
        _nextSibling = default;

        World.MarkHierarchyDirty();
    }

    /// <summary>
    /// Unparents this entity, making it top level at the current <see cref="EntityRegistry"/>
    /// This is the same state entities are when they're created without a parent
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnParent()
    {
        UnlinkFromParent();
        ActiveInHierarchy = true;
    }

    /// <summary>
    /// Whether this Entity is active in the hierarchy
    /// </summary>
    public bool ActiveSelf
    {
        readonly get;
        set
        {
            if (field == value)
                return;

            field = value;
            UpdateActiveInHierarchy();
        }
    }

    private void UpdateActiveInHierarchy()
    {
        ActiveInHierarchy =
            ActiveSelf &&
            (_parent.Id == 0 || World.GetEntity(_parent).ActiveInHierarchy);

        var child = _firstChild;

        while (child.Id != 0)
        {
            ref Entity entity = ref World.GetEntity(child);

            entity.UpdateActiveInHierarchy();

            child = entity._nextSibling;
        }
    }

    /// <summary>
    /// Whether this Entity is active in the scene
    /// Say, Entity A has a child Entity B. B could be active but A 
    /// be inactive, which would result in B <see cref="ActiveInHierarchy"/> be false
    /// while <see cref="ActiveSelf"/> is true
    /// </summary>
    public bool ActiveInHierarchy
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => ActiveSelf && field;
        internal set 
        {
            if (field == value)
                return;

            field = value;

            foreach (var typedComponent in _componentList.Components)
                World.EntityActiveInHierarchyChanged(ActiveInHierarchy, typedComponent.type, typedComponent.handle);
        }
    }

    private ulong _lastWriteTick;
    private ulong _renderFrame;

    private Transform2D _previous;
    [SerializeField] private Transform2D _current= new();
    private Transform2D _renderTransform ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCurrentTick()
    {
        if (_lastWriteTick != World.FixedTick)
        {
            _previous = _current;
            _lastWriteTick = World.FixedTick;
        }
    }

    /// <summary>
    /// The transform of this entity
    /// This returns a copy, to modify the position,
    /// rotation or scale use the direct properties
    /// </summary>
    public Transform2D Transform
    {
        get
        {
            if (World.IsDrawing)
                return RenderTransform;

            return _current;
        }

        set
        {
            EnsureCurrentTick();
            _current = value;
        }
    }

    /// <summary>
    /// Gets/Sets this entity position
    /// </summary>
    public Vector2 Position
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (World.IsDrawing)
                return RenderTransform.Position;

            return _current.Position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            EnsureCurrentTick();
            _current.Position = value;
        }
    }

    /// <summary>
    /// Gets/Set this entity rotation in radians
    /// </summary>
    public float Rotation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (World.IsDrawing)
                return RenderTransform.Rotation;

            return _current.Rotation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            EnsureCurrentTick();
            _current.Rotation = value;
        }
    }

    /// <summary>
    /// Gets/Set this entity rotation in degrees
    /// </summary>
    public float RotationDegrees
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => MathHelper.ToDegrees(_current.Rotation);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            EnsureCurrentTick();
            _current.Rotation = MathHelper.ToRadians(value);
        }
    }


    /// <summary>
    /// Gets/Set this entity scale
    /// </summary>
    public Vector2 Scale
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (World.IsDrawing)
                return RenderTransform.Scale;
            
            return _current.Scale;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            EnsureCurrentTick();
            _current.Scale = value;
        }
    }

    /// <summary>
    /// Teleports this entity to the position <paramref name="position"/>
    /// bypassing physics interpolation
    /// </summary>
    /// <param name="position"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Teleport(Vector2 position)
    {
        _current.Position = position;
        _previous.Position = position;
        _renderTransform.Position = position;
    }

    /// <summary>
    /// Resets physics interpolation for this frame
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetInterpolation()
    {
        _previous = _current;
        _renderFrame = ulong.MaxValue;
    }

    private Transform2D RenderTransform
    {
        get
        {
            if (_renderFrame != World.RenderFrame)
            {
                _renderTransform = Interpolate(_previous, _current, EntityRegistry.FixedUpdateAlpha);
                _renderFrame = World.RenderFrame;
            }

            return _renderTransform;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Transform2D Interpolate(Transform2D previous, Transform2D current,float alpha)
    {
        return new Transform2D
        {
            Position = Vector2.Lerp(previous.Position, current.Position, alpha),
            Rotation = LerpAngle(previous.Rotation, current.Rotation, alpha),
            Scale = Vector2.Lerp(previous.Scale,current.Scale,alpha)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float LerpAngle(float from, float to, float alpha)
    {
        float delta = MathF.IEEERemainder(to - from, MathF.Tau);
        return from + delta * alpha;
    }

    /// <summary>
    /// Creates an entity without name, flags and 0 components
    /// </summary>
    public Entity()
    {
    }

    /// <summary>
    /// Adds a component to the entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref T AddComponent<T>() where T : struct, IComponent, IHandle<T> => ref AddComponent<T>(new());

    /// <summary>
    /// Adds a component to the entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="component"></param>
    /// <returns></returns>
    public readonly ref T AddComponent<T>(T component) where T : struct, IComponent, IHandle<T>
    {
        component.World = World;
        component.EntityHandle = Handle;

        ref T componentRef = ref World.CreateComponent(Handle, component);
        _componentList.Add<T>(componentRef.Handle);

        return ref componentRef;
    }

    /// <summary>
    /// Removes the first occurrence of the component of type T from this entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public readonly bool RemoveComponent<T>() where T : struct, IComponent
    {
        Handle removedHandle = _componentList.Remove<T>();
        if (removedHandle.Id == 0)
            return false;

        bool worldRemoves = World.RemoveComponent(ComponentTypeId<T>.Id, removedHandle);
        bool entityRemoves = removedHandle.Id != 0;

        if (!worldRemoves && entityRemoves)
            Log.Warning($"Component of types {typeof(T)} with handle {removedHandle} couldn't be removed from World but it was removed from entityt");

        return worldRemoves && entityRemoves;
    }

    /// <summary>
    /// Removes the component whose handle is <paramref name="handle"/> from this entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="handle"></param>
    /// <returns></returns>
    public readonly bool RemoveComponent<T>(Handle<T> handle) where T : struct, IComponent, IHandle<T>
    {
        bool worldRemoves = World.RemoveComponent<T>(handle);
        var removedHandle = _componentList.Remove<T>(handle);
        bool entityRemoves = removedHandle.Id != 0;

        if (worldRemoves && !entityRemoves)
            Log.Warning($"Component of types {typeof(T)} with handle {handle} couldn't be removed from entity but it could be removed from World");

        if (!worldRemoves && entityRemoves)
            Log.Warning($"Component of types {typeof(T)} with handle {handle} couldn't be removed from World but it was removed from entityt");

        return worldRemoves && entityRemoves;
    }

    /// <summary>
    /// Gets the handle to the first occurence of the component of type T
    /// Returns an invalid handle if the component wasn't in the entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Handle<T> GetComponentHandle<T>() where T : struct, IComponent => _componentList.Get<T>();

    /// <summary>
    /// Gets the first occurrence of the component of type T
    /// Returns an invalid component if it wasn't present in this entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref T GetComponent<T>() where T : struct, IComponent, IHandle<T> => ref World.GetComponent<T>(GetComponentHandle<T>());

    /// <summary>
    /// Gets the component whose handle is <paramref name="componentHandle"/> from this entity
    /// Returns an invalid component if none was found
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="componentHandle"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref T GetComponent<T>(Handle<T> componentHandle) where T : struct, IComponent, IHandle<T> => ref World.GetComponent<T>(componentHandle);

    /// <summary>
    /// Checks if the entity has a component of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool HasComponent<T>() where T : struct, IComponent => GetComponentHandle<T>().Id != 0;

    /// <summary>
    /// Checks if the entity has the component pointed by <paramref name="componentHandle"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="componentHandle"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool HasComponent<T>(Handle<T> componentHandle) where T : struct, IComponent => _componentList.Has(componentHandle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly void Release()
    {
        foreach (var typedComponent in _componentList.Components)
            World.RemoveComponent(typedComponent.type, typedComponent.handle);

        _componentList.Clear();
    }

    /// <inheritdoc/>
    public struct ChildEnumerator : IEnumerator<Entity>
    {
        private readonly EntityRegistry _world;
        private readonly Handle<Entity> _firstChild;
        private Handle<Entity> _current;
        private bool _started;

        /// <inheritdoc/>
        public readonly ref Entity Current => ref _world.GetEntity(_current);
        readonly Entity IEnumerator<Entity>.Current => _world.GetEntity(_current);
        readonly object IEnumerator.Current => Current;

        internal ChildEnumerator(EntityRegistry world, Handle<Entity> firstChild)
        {
            _world = world;
            _firstChild = firstChild;
            _current = default;
            _started = false;
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (!_started)
            {
                _started = true;
                _current = _firstChild;
            }
            else if (_current.Id != 0)
            {
                _current = _world.GetEntity(_current)._nextSibling;
            }

            return _current.Id != 0;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _current = default;
            _started = false;
        }

        /// <inheritdoc/>
        public readonly void Dispose() { }
    }

    /// <inheritdoc/>
    public readonly struct ChildEnumerable
    {
        private readonly EntityRegistry _world;
        private readonly Handle<Entity> _firstChild;

        internal ChildEnumerable(EntityRegistry world, Handle<Entity> firstChild)
        {
            _world = world;
            _firstChild = firstChild;
        }

        /// <inheritdoc/>
        public ChildEnumerator GetEnumerator() => new(_world, _firstChild);
    }

    /// <inheritdoc/>
    public readonly ChildEnumerable Children => new(World, _firstChild);
}
