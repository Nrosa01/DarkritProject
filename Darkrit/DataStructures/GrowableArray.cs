using Darkrit.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Darkrit.DataStructures;

public sealed class GrowableArray<T> : IEnumerable<T>, IEnumerable
{
    private T[] _items = [];
    private int _count;

    public int Count => _count;
    public int Capacity => _items.Length;

    public GrowableArray(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _items = new T[capacity];
    }

    public GrowableArray() : this(4) { }

    public ReadOnlySpan<T> AsReadOnlySpan() => _items.AsSpan(0, _count);

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        if (_count == _items.Length)
            Grow();

        _items[_count++] = item;
    }

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

            if ((uint)index < (uint)_count)
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

    public ref T this[int index] => ref _items[index];
}