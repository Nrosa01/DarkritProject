using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using Darkrit.Base;
using Darkrit.Physics.Boxy2D;

namespace Darkrit.DataStructures;

public class HandleMapGrowing<T> : IEnumerable<HandleItem<T>>, IEnumerable<T> where T : new()
{
    public delegate void IterationAction(Handle<T> handle, ref T item);

    readonly GrowableArray<HandleItem<T>> _items;
    private readonly Stack<int> _deletedItems = new();

    public int Count => _items.Count - _deletedItems.Count - 1;

    int _nextItem = 0;

    public HandleMapGrowing() : this(256) { }

    public HandleMapGrowing(int capacity = 256)
    {
        _items = new (capacity)
        {
            // First element is the invalid element.
            default
        };
    }

    public ReadOnlySpan<HandleItem<T>> Items => _items.AsReadOnlySpan();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int NextId()
    {
        if (_deletedItems.TryPop(out var result))
            return result;
        else
            return (++_nextItem);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<T> Add(T item)
    {
        var nextId = NextId();

        if (nextId < _items.Count)
            _items[nextId] = new HandleItem<T>
            {
                Handle = new Handle<T>
                {
                    Id = nextId,
                    Generation = _items[nextId].Handle.Generation + 1 // Previous generation
                },
                Item = item
            };
        else
        {
            _items.Add(new HandleItem<T>
            {
                Handle = new Handle<T>
                {
                    Id = nextId,
                    Generation = 0
                },
                Item = item
            });
        }

        return _items[nextId].Handle;
    }

    /// <summary>
    /// Gets a reference to the stored item.
    /// This reference shouldn't be stored, as the underlaying array
    /// can resize at any time
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    /// 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get(Handle<T> handle)
    {
        Debug.Assert(IsValid(handle));
        return ref this[handle];
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsValid(Handle<T> handle) => handle.Id > 0 && handle.Id < _items.Count && _items[handle.Id].Handle == handle;

    public ref T this[int index] => ref _items[index].Item;

    public ref T this[Handle<T> handle] => ref _items[handle.Id].Item;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(Handle<T> handle)
    {
        if (!IsValid(handle)) return false;

        // Mark as invalid so it's skipped during iteration
        ref var item = ref _items[handle.Id];
        item.Handle = item.Handle with { Id = 0 };

        _deletedItems.Push(handle.Id);
        return true;
    }

    public struct Enumerator : IEnumerator<HandleItem<T>>, IEnumerator<T>
    {
        private readonly GrowableArray<HandleItem<T>> _items;
        private int _index;

        internal Enumerator(GrowableArray<HandleItem<T>> items)
        {
            _items = items;
            _index = -1;
        }

        public readonly ref HandleItem<T> Current => ref _items[_index];

        readonly HandleItem<T> IEnumerator<HandleItem<T>>.Current => _items[_index];
        readonly T IEnumerator<T>.Current => _items[_index].Item;

        readonly object IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (++_index < _items.Count)
            {
                if (_items[_index].Handle.Id != 0)
                    return true;
            }

            return false;
        }

        public void Reset() => _index = -1;

        public readonly void Dispose() { }
    }

    IEnumerator<HandleItem<T>> IEnumerable<HandleItem<T>>.GetEnumerator() => GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public Enumerator GetEnumerator() => new(_items);
}
