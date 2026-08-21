// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Darkrit.Base;
using Darkrit.DevTools.Logger;
using Darkrit.Math;
using Microsoft.Xna.Framework;
using Handle = Darkrit.Base.Handle;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Matrix = Microsoft.Xna.Framework.Matrix;

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

public struct Entity
{
    public int Flags;

    public StringID NameID { readonly get; internal set; }

    public string Name
    {
        get => NameID.ToString();
        set => NameID = new(value);
    }

    public EntityRegistry World { get; init; }

    readonly ComponentList _componentList = new();

    public readonly IReadOnlyList<TypedHandle> Components => _componentList.Components;

    public readonly Handle<Entity> Handle { get; internal init; }

    internal Handle<Entity> _parent;
    internal Handle<Entity> _firstChild;
    internal Handle<Entity> _lastChild;
    internal Handle<Entity> _nextSibling;
    internal Handle<Entity> _previousSibling;
    private int _childCount;

    public ref Entity Parent => ref World.GetEntity(_parent);

    public readonly bool HasParent => _parent.Id != 0;

    public readonly int ChildCount => _childCount;

    public bool TrySetParentFirst(Handle<Entity> newParent)
    {
        if (_parent == newParent || WouldCreateCycle(newParent))
            return false;

        UnlinkFromParent();

        if (newParent.Id == 0)
        {
            ActiveInHierachy = ActiveSelf;
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

    public bool TrySetParent(Handle<Entity> newParent)
    {
        if(_parent == newParent || WouldCreateCycle(newParent))
            return false;

        UnlinkFromParent();

        if (newParent.Id == 0)
        {
            ActiveInHierachy = ActiveSelf;
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

    private bool WouldCreateCycle(Handle<Entity> newParent)
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

    public bool TryAddChild(Handle<Entity> child) => World.GetEntity(child).TrySetParent(Handle);

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

    public void UnParent()
    {
        UnlinkFromParent();
        ActiveInHierachy = true;
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
        ActiveInHierachy =
            ActiveSelf &&
            (_parent.Id == 0 || World.GetEntity(_parent).ActiveInHierachy);

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
    /// be inactive, which would result in B <see cref="ActiveInHierachy"/> be false
    /// while <see cref="ActiveSelf"/> is true
    /// </summary>
    public bool ActiveInHierachy
    {
        readonly get => ActiveSelf && field;
        internal set 
        {
            field = value;

            foreach (var typedComponent in _componentList.Components)
                World.ComponentEnabledCallback(field, typedComponent.type, typedComponent.handle);
        }
    }

    private ulong _lastWriteTick;
    private ulong _renderFrame;

    private Transform2D _previous;
    private Transform2D _current;
    private Transform2D _renderTransform;

    private void EnsureCurrentTick()
    {
        if (_lastWriteTick != World.Tick)
        {
            _previous = _current;
            _lastWriteTick = World.Tick;
        }
    }

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

    public Vector2 Position
    {
        get
        {
            if (World.IsDrawing)
                return RenderTransform.Position;

            return _current.Position;
        }

        set
        {
            EnsureCurrentTick();
            _current.Position = value;
        }
    }

    public float Rotation
    {
        get
        {
            if (World.IsDrawing)
                return RenderTransform.Rotation;

            return _current.Rotation;
        }

        set
        {
            EnsureCurrentTick();
            _current.Rotation = value;
        }
    }

    public float RotationDegrees
    {
        get => MathHelper.ToDegrees(_current.Rotation);

        set
        {
            EnsureCurrentTick();
            _current.Rotation = MathHelper.ToRadians(value);
        }
    }

    public Vector2 Scale
    {
        get
        {
            if (World.IsDrawing)
                return RenderTransform.Scale;
            
            return _current.Scale;
        }

        set
        {
            EnsureCurrentTick();
            _current.Scale = value;
        }
    }

    public void Teleport(Vector2 position)
    {
        _current.Position = position;
        _previous.Position = position;
        _renderTransform.Position = position;
    }

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
                _renderTransform = Interpolate(_previous, _current, World.FixedUpdateAlpha);
                _renderFrame = World.RenderFrame;
            }

            return _renderTransform;
        }
    }

    private static Transform2D Interpolate(Transform2D previous, Transform2D current,float alpha)
    {
        return new Transform2D
        {
            Position = Vector2.Lerp(previous.Position, current.Position, alpha),
            Rotation = LerpAngle(previous.Rotation, current.Rotation, alpha),
            Scale = Vector2.Lerp(previous.Scale,current.Scale,alpha)
        };
    }

    private static float LerpAngle(float from, float to, float alpha)
    {
        float delta = MathF.IEEERemainder(to - from, MathF.Tau);
        return from + delta * alpha;
    }

    public Entity()
    {
    }

    public Handle<T> AddComponent<T>() where T : struct, IComponent => AddComponent<T>(new());

    public Handle<T> AddComponent<T>(T component) where T : struct, IComponent
    {
        component.World = World;
        component.EntityHandle = Handle;

        Handle<T> componentHandle = World.CreateComponent(Handle, component);

        _componentList.Add<T>(componentHandle);

        return componentHandle;
    }

    public bool RemoveComponent<T>() where T : struct, IComponent
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

    public bool RemoveComponent<T>(Handle<T> handle) where T : struct, IComponent
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

    public Handle<T> GetComponentHandle<T>() where T : struct, IComponent => _componentList.Get<T>();

    public ref T GetComponent<T>() where T : struct, IComponent => ref World.GetComponent<T>(GetComponentHandle<T>());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetComponent<T>(Handle<T> componentHandle) where T : struct, IComponent => ref World.GetComponent<T>(componentHandle);

    public bool HasComponent<T>() where T : struct, IComponent => GetComponentHandle<T>().Id != 0;
    public bool HasComponent<T>(Handle<T> componentHandle) where T : struct, IComponent => _componentList.Has(componentHandle);

    internal readonly void Release()
    {
        foreach (var typedComponent in _componentList.Components)
            World.RemoveComponent(typedComponent.type, typedComponent.handle);

        _componentList.Clear();
    }

    public struct ChildEnumerator : IEnumerator<Entity>
    {
        private readonly EntityRegistry _world;
        private readonly Handle<Entity> _firstChild;
        private Handle<Entity> _current;
        private bool _started;

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

        public void Reset()
        {
            _current = default;
            _started = false;
        }

        public void Dispose() { }
    }

    public readonly struct ChildEnumerable
    {
        private readonly EntityRegistry _world;
        private readonly Handle<Entity> _firstChild;

        internal ChildEnumerable(EntityRegistry world, Handle<Entity> firstChild)
        {
            _world = world;
            _firstChild = firstChild;
        }

        public ChildEnumerator GetEnumerator() => new(_world, _firstChild);
    }

    public readonly ChildEnumerable Children => new(World, _firstChild);
}
