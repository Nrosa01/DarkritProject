using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Darkrit.Base;

namespace Darkrit.DataStructures;


/// <summary>
/// Dynamic array that stores item as <see cref="HandleItem{T}"/>
/// This allows to store items as a better alternative to pointers, the memory
/// can be reused and the handle allows to know if the items still exists safely
/// 
/// Despite it's called a "map", it stores handles as a <see cref="GrowableArray{HandleItem{T}}"/> internally
/// and most operations are O(1)
/// </summary>
/// <typeparam name="T">The type of the object to be stored in a <see cref="HandleItem{T}"/></typeparam>
public class HandleMapGrowing<T> : IEnumerable<HandleItem<T>>, IEnumerable<T> where T : new()
{
    readonly GrowableArray<HandleItem<T>> _items;
    private readonly Stack<int> _deletedItems = new();

    /// <summary>
    /// Gets the number of valid elements contained in the <see cref="HandleMapGrowing{T}"/>
    /// </summary>
    public int Count => _items.Count - _deletedItems.Count - 1;

    int _nextItem = 0;

    public HandleMapGrowing() : this(256) { }


    /// <summary>
    /// Initializes a <see cref="HandleMapGrowing{T}"/> with 0 elements and <paramref name="capacity"/> allocated elements
    /// </summary>
    /// <param name="capacity">Number of elements to preallocate</param>
    public HandleMapGrowing(int capacity = 256)
    {
        _items = new (capacity)
        {
            // First element is the invalid element.
            default
        };
    }

    /// <summary>
    /// Creates a ReadOnlySpan over the contained elements
    /// </summary>
    /// <returns>The ReadOnlySpan view</returns>
    public ReadOnlySpan<HandleItem<T>> Items => _items.AsReadOnlySpan();

    /// <summary>
    /// Gets the next valid id, it reuses ids from deleted items
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetNextId()
    {
        if (_deletedItems.TryPop(out var result))
            return result;
        else
            return (++_nextItem);
    }

    /// <summary>
    /// Returns what the next handle would be. Useful if you need to
    /// store the handle in your returned type but you need to do so
    /// in the constructor
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<T> PeekNextId()
    {
        int nextId;
        if (_deletedItems.TryPeek(out var result))
            nextId = result;
        else
            nextId = (_nextItem + 1);

        
        if (nextId < _items.Count)
            return new Handle<T>
            {
                Id = nextId,
                Generation = _items[nextId].Handle.Generation + 1 // Previous generation
            };
        else
        {
            return new Handle<T>
            {
                Id = nextId,
                Generation = 0
            };
        }
    }

    /// <summary>
    /// Adds an object to the end of the <see cref="HandleMapGrowing{T}"/>
    /// </summary>
    /// <param name="item">The object to be added to the end of the <see cref="HandleMapGrowing{T}"/>. 
    /// The value can be null for reference types.</param>
    /// <returns>The <see cref="Handle{T}"/> that maps to the real item in the map</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<T> Add(T item)
    {
        var nextId = GetNextId();

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
    /// 
    /// This is the same as doing <see cref="this[Handle{T}]"/>
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get(Handle<T> handle) => ref this[handle];

    /// <summary>
    /// Checks that the <see cref="Handle{T}"/> is valid. That means:
    /// <para>- Its id must not be 0</para>
    /// - Its generation must match the one at this[handle.id]
    /// - Id must be less that the amount of contained items (valid or invalid)
    /// </summary>
    /// <param name="handle">The handle to check</param>
    /// <returns>Wheher the handle is valid</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsValid(Handle<T> handle) => handle.Id > 0 && handle.Id < _items.Count && _items[handle.Id].Handle == handle;

    /// <summary>
    /// Gets or sets the element at the specified index
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns></returns>
    private ref T this[int index] => ref _items[index].Item;

    /// <summary>
    /// Gets or sets the element at the specified handle. 
    /// </summary>
    /// <remarks>
    /// In Debug it checks that the <see cref="Handle{T}"/> 
    /// <para></para>
    /// is valid <seealso cref="IsValid(Handle{T})"/>
    /// </remarks>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns></returns>
    public ref T this[Handle<T> handle]
    {
        get
        {
            Debug.Assert(IsValid(handle));
            return ref _items[handle.Id].Item;
        }
    }

    /// <summary>
    /// Removes the item associated with <paramref name="handle"/> if it's valid
    /// </summary>
    /// <param name="handle">The handle that maps to the item being removed</param>
    /// <returns>Returns true if the item associated with <paramref name="handle"/> was removed</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(Handle<T> handle)
    {
        if (!IsValid(handle)) return false;

        // Mark as invalid so it's skipped during iteration
        ref var item = ref _items[handle.Id];
        item.Handle = item.Handle with { Id = 0 };

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            item.Item = default; // Sets to null if it contains a reference type

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
