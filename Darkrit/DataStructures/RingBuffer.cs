using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.DataStructures
{
    public class RingBuffer<T>(int capacity) : IEnumerable<T>
    {
        private readonly T[] _buffer = new T[capacity];
        private int _end;
        private int _start;
        private int _size;

        public void PushFront(T item)
        {
            _end = _end % _buffer.Length;
            if (_end == _start) _start = (_start + 1) % _buffer.Length;

            _buffer[_end++] = item;
        }

        public void PushBack(T item)
        {

        }

        public int Size => _size;

        public int Capacity => _buffer.Length;

        public bool IsFull => Size == Capacity;

        public bool IsEmpty => Size == 0;

        public T Front()
        {
            return _buffer[_start];
        }

        public T Back()
        {
            return _buffer[(_end != 0 ? _end : _size];
        }

        public void PopFront()
        {

        }

        public void PopBack()
        {

        }

        public T this[int index]
        {

        }

        // Converts an ordinal index to the ring buffer taking into account start and end
        private int RealIndex(int index)
        {

        }

        public void Clear()
        {
            _end = 0;
            _start = 0;
            _size = 0;
            Array.Clear(_buffer, 0, _buffer.Length);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Capacity; i++)
            {
                int realIndex = RealIndex(i);
                yield return _buffer[realIndex];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
