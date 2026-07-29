using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

// Source: https://github.com/joaoportela/CircularBuffer-CSharp/blob/master/CircularBuffer/CircularBuffer.cs
// This implementation was first written without the source, then changed to it because it was much better
// There might be some function I didn't copy but in its current state everything is mostly from there

namespace Darkrit.DataStructures
{
    /// <summary>
    /// Stores a fixed number of items, wrapping around when it's full
    /// </summary>
    /// <typeparam name="T">Type of the buffer item</typeparam>
    public class RingBuffer<T> : IEnumerable<T>
    {
        private readonly T[] _buffer;
        private int _end;
        private int _start;
        private int _size;

        /// <summary>
        /// Constructs an empty buffer.
        /// </summary>
        /// <param name="capacity">Capacity of the buffer</param>
        public RingBuffer(int capacity) :this(capacity, []) {}

        /// <summary>
        /// Creates a buffer given an existing array of items. Items will be copied to the buffer
        /// </summary>
        /// <param name="capacity">Capacity of the buffer</param>
        /// <param name="items">List of items the buffer will start with</param>
        /// <exception cref="ArgumentException">If the capacity is 0 or negative</exception>
        /// <exception cref="ArgumentNullException">It the item list provided is null</exception>
        public RingBuffer(int capacity, T[] items)
        {
            if (capacity < 1)
                throw new ArgumentException("Ring buffer can't have negative or zero capacity.", nameof(capacity));

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (items.Length > capacity)
                throw new ArgumentException(
                    "Too many items to fit circular buffer", nameof(items));

            _buffer = new T[capacity];

            Array.Copy(items, _buffer, items.Length);
            _size = items.Length;

            _start = 0;
            _end = _size == capacity ? 0 : _size;
        }

        /// <summary>
        /// Pushes an element to the front of the buffer.
        /// 
        /// When the buffer is full, the element at Back will be popped
        /// for this new element to fit
        /// </summary>
        /// <param name="item">Item to push to the front of the buffer</param>
        public void PushFront(T item)
        {
            if (IsFull)
            {
                Decrement(ref _start);
                _end = _start;
                _buffer[_start] = item;
            }
            else
            {
                Decrement(ref _start);
                _buffer[_start] = item;
                ++_size;
            }
        }

        /// <summary>
        /// Increments the provided index variable by one, wrapping
        /// around if necessary.
        /// </summary>
        /// <param name="index"></param>
        private void Increment(ref int index)
        {
            if (++index == Capacity)
            {
                index = 0;
            }
        }

        /// <summary>
        /// Decrements the provided index variable by one, wrapping
        /// around if necessary.
        /// </summary>
        /// <param name="index"></param>
        private void Decrement(ref int index)
        {
            if (index == 0)
            {
                index = Capacity;
            }
            index--;
        }

        public void PushBack(T item)
        {
            if (IsFull)
            {
                _buffer[_end] = item;
                Increment(ref _end);
                _start = _end;
            }
            else
            {
                _buffer[_end] = item;
                Increment(ref _end);
                ++_size;
            }
        }

        /// <summary>
        /// Current number of elements in use in the buffer
        /// </summary>
        public int Size => _size;

        /// <summary>
        /// Total capacity of the buffer
        /// </summary>
        public int Capacity => _buffer.Length;

        /// <summary>
        /// Whether the buffer is full or not. If full, appending elements
        /// will make others pop to allow the new one to fit
        /// </summary>
        public bool IsFull => Size == Capacity;

        /// <summary>
        /// Whether the buffer is empty
        /// </summary>
        public bool IsEmpty => Size == 0;

        /// <summary>
        /// Element at the front of the buffer. Equivalent to this[0]
        /// </summary>
        /// <returns>The value of the element of type T at the front of the buffer</returns>
        public T Front()
        {
            ThrowIfEmpty();

            return _buffer[_start];
        }



        /// <summary>
        /// Element at the front of the buffer. Equivalent to this[Size - 1]
        /// </summary>
        /// <returns>The value of the element of type T at the end of the buffer</returns>
        public T Back()
        {
            ThrowIfEmpty();

            return _buffer[(_end != 0 ? _end : Capacity) - 1];
        }

        private void ThrowIfEmpty()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Buffer is empty");
        }

        /// <summary>
        /// Pops the Front element of the buffer and returns it.
        /// Front is equivalent to this[0]
        /// </summary>
        /// <returns>The element of type T at the front of the buffer</returns>
        public T PopFront()
        {
            ThrowIfEmpty();

            var front = Front();
            _buffer[_start] = default;
            Increment(ref _start);
            _size--;
            return front;
        }

        /// <summary>
        /// Pops the Back element of the buffer and returns it.
        /// Front is equivalent to this[Size - 1]
        /// </summary>
        /// <returns>The element of type T at the back of the buffer</returns>
        public T PopBack()
        {
            ThrowIfEmpty();

            var back = Back();
            Decrement(ref _end);
            _buffer[_end] = default;
            _size--;
            return back;
        }

        public T this[int index]
        {
            get
            {
                if (IsEmpty)
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty");
                if (index >= _size)
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer size is {_size}");
                int actualIndex = RealIndex(index);
                return _buffer[actualIndex];
            }
            set
            {
                if (IsEmpty)
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty");
                if (index >= _size)
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer size is {_size}");
                int actualIndex = RealIndex(index);
                _buffer[actualIndex] = value;
            }
        }

        // Converts an ordinal index to the ring buffer taking into account start and end
        private int RealIndex(int index) => _start + (index < (Capacity - _start) ? index : index - Capacity);

        /// <summary>
        /// Clears the buffer. It's not just setting <see cref="Size"/> to 0. It actually overwrittes the buffer content with 0s
        /// </summary>
        public void Clear()
        {
            _end = 0;
            _start = 0;
            _size = 0;
            Array.Clear(_buffer, 0, _buffer.Length);
        }

        /// <summary>
        /// Returns a readonly view of the inner buffer as two segments
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<ArraySegment<T>> ToArraySegments() => [ArrayOne(), ArrayTwo()];

        public IEnumerator<T> GetEnumerator()
        {
            var segments = ToArraySegments();
            foreach (ArraySegment<T> segment in segments)
            {
                for (int i = 0; i < segment.Count; i++)
                    yield return segment.Array[segment.Offset + i];
            }
        }

        /// <summary>
        /// Copies the buffer contents to an array, according to the logical
        /// contents of the buffer (i.e. independent of the internal 
        /// order/contents)
        /// </summary>
        /// <returns>A new array with a copy of the buffer contents.</returns>
        public T[] ToArray()
        {
            T[] newArray = new T[Size];
            int newArrayOffset = 0;
            var segments = ToArraySegments();
            foreach (ArraySegment<T> segment in segments)
            {
                Array.Copy(segment.Array, segment.Offset, newArray, newArrayOffset, segment.Count);
                newArrayOffset += segment.Count;
            }
            return newArray;
        }

        private ArraySegment<T> ArrayOne()
        {
            if (IsEmpty)
                return new ArraySegment<T>([]);
            else if (_start < _end)
                return new ArraySegment<T>(_buffer, _start, _end - _start);
            else
                return new ArraySegment<T>(_buffer, _start, _buffer.Length - _start);
        }

        private ArraySegment<T> ArrayTwo()
        {
            if (IsEmpty)
                return new ArraySegment<T>([]);
            else if (_start < _end)
                return new ArraySegment<T>(_buffer, _end, 0);
            else
                return new ArraySegment<T>(_buffer, 0, _end);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
