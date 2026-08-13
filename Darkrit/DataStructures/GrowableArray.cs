using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Darkrit.DataStructures;

/// <summary>
/// Dynamic array that resizes when adding items
/// Unlike <see cref="List{T}"/>, it's possible to get the underlaying value by reference
/// so there is no need to convert to a Span which saves processing time in hotpaths
/// </summary>
/// <typeparam name="T">Type to be contained</typeparam>
public sealed class GrowableArray<T> : IEnumerable<T>, IEnumerable
{
    private T[] _items = [];
    private int _count;

    /// <summary>
    /// Gets the number of elements contained in the <see cref="GrowableArray{T}"/> 
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// The total number of elements the internal data structure can hold without resizing
    /// </summary>
    public int Capacity => _items.Length;

    /// <summary>
    /// Initializes a <see cref="GrowableArray{T}"/> with 0 elements and <paramref name="capacity"/> allocated elements
    /// </summary>
    /// <param name="capacity">Number of elements to preallocate</param>
    public GrowableArray(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _items = new T[capacity];
    }

    public GrowableArray() : this(4) { }

    /// <summary>
    /// Creates a ReadOnlySpan over the contained elements
    /// </summary>
    /// <returns>The ReadOnlySpan view</returns>
    public ReadOnlySpan<T> AsReadOnlySpan() => _items.AsSpan(0, _count);

    /// <summary>
    /// Remove all elements from the <see cref="GrowableArray{T}"/>
    /// <remarks>
    /// <see cref="Count"/> is set to 0, and references to other objects 
    /// from elements of the collection are also released.
    /// <see cref="Capacity"/> remains unchanged
    /// This method is an O(n) operation, where n is <see cref="Count"/> only if <see cref="T"/> is or contains references.
    /// Pure value types aren't cleared
    /// </remarks>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (_count == 0)
            return;

        // If T is a struct that doesn't hold any ref, there is no need to clear it
        // This can save some performance on big collections in hotpath
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            Array.Clear(_items, 0, _count);

        _count = 0;
    }


    /// <summary>
    /// Adds an object to the end of the <see cref="GrowableArray{T}"/>
    /// </summary>
    /// <param name="item">The object to be added to the end of the <see cref="GrowableArray{T}"/>. 
    /// The value can be null for reference types.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        if (_count == _items.Length)
            Grow();

        _items[_count++] = item;
    }


    /// <summary>
    /// Reizes the array duplicating its <see cref="Capacity"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow()
    {
        int newCapacity = _items.Length == 0 ? 4 : _items.Length * 2;

        Array.Resize(ref _items, newCapacity);
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly T[] _items;
        private readonly int _count;
        private int _index;

        internal Enumerator(T[] items, int count)
        {
            _items = items;
            _count = count;
            _index = -1;
        }

        public readonly ref T Current => ref _items[_index];

        readonly T IEnumerator<T>.Current => _items[_index];

        readonly object IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            int index = _index + 1;

            if (index < _count)
            {
                _index = index;
                return true;
            }

            return false;
        }

        public void Reset() => _index = -1;

        public readonly void Dispose() { }
    }

    public Enumerator GetEnumerator() => new(_items, _count);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns></returns>
    public ref T this[int index] => ref _items[index];
}