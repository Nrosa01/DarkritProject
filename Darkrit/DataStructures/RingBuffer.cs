using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.DataStructures
{
    public class RingBuffer<T> : IEnumerable<T>
    {
        private readonly T[] _buffer;
        private int _end;
        private int _start;
        private int _size;

        public RingBuffer(int capacity) :this(capacity, []) {}

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

        public int Size => _size;

        public int Capacity => _buffer.Length;

        public bool IsFull => Size == Capacity;

        public bool IsEmpty => Size == 0;

        public T Front()
        {
            ThrowIfEmpty();

            return _buffer[_start];
        }

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

        public T PopFront()
        {
            ThrowIfEmpty();

            var front = Front();
            _start--;
            return front;
        }

        public T PopBack()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Buffer is empty");

            var back = Back();
            _end++;
            return back;
        }

        public T this[int index]
        {
            get
            {
                ThrowIfEmpty();


                int internalIndex = RealIndex(index);

                if (!ValidIndex(internalIndex, out string error))
                    throw new InvalidOperationException(error);

                return _buffer[index];
            }
            set
            {
                ThrowIfEmpty();

                int internalIndex = RealIndex(index);
                
                if (!ValidIndex(internalIndex, out string error))
                    throw new InvalidOperationException(error);

                _buffer[index] = value;
            }
        }

        // Converts an ordinal index to the ring buffer taking into account start and end
        private int RealIndex(int index)
        {
            return _start + (index < (Capacity - _start) ? index : index - Capacity);
        }

        private bool ValidIndex(int internalIndex, out string error)
        {
            if (internalIndex >= _end || internalIndex < _start)
            {
                error = "Index out of bounds";
                return false;
            }

            error = null;
            return true;
        }

        public void Clear()
        {
            _end = 0;
            _start = 0;
            _size = 0;
            Array.Clear(_buffer, 0, _buffer.Length);
        }
        public IList<ArraySegment<T>> ToArraySegments()
        {
            return [ArrayOne(), ArrayTwo()];
        }

        public IEnumerator<T> GetEnumerator()
        {
            var segments = ToArraySegments();
            foreach (ArraySegment<T> segment in segments)
            {
                for (int i = 0; i < segment.Count; i++)
                {
                    yield return segment.Array[segment.Offset + i];
                }
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
            {
                return new ArraySegment<T>(new T[0]);
            }
            else if (_start < _end)
            {
                return new ArraySegment<T>(_buffer, _start, _end - _start);
            }
            else
            {
                return new ArraySegment<T>(_buffer, _start, _buffer.Length - _start);
            }
        }

        private ArraySegment<T> ArrayTwo()
        {
            if (IsEmpty)
            {
                return new ArraySegment<T>(new T[0]);
            }
            else if (_start < _end)
            {
                return new ArraySegment<T>(_buffer, _end, 0);
            }
            else
            {
                return new ArraySegment<T>(_buffer, 0, _end);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
