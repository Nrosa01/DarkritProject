using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Darkrit.Physics.Boxy2D;

public class HandleMapGrowing<T> where T : new()
{
    public readonly struct Handle
    {
        public int Id { get; init; }
        public int Generation { get; init; }
    }

    List<T> _items;
    List<int> _generations;
    private readonly Stack<int> _deletedItems = new();

    int _nextItem = 0;

    public HandleMapGrowing(int capacity = 256)
    {
        _items = new(capacity);
        _generations = new(capacity);

        // First element is the invalid element.
        _items.Add(default);
        _generations.Add(default);
    }

    public IReadOnlyList<T> Items => _items;

    private int NextId()
    {
        if (_deletedItems.TryPop(out var result))
            return result;
        else
            return (++_nextItem);
    }

    public Handle Add(T item)
    {
        var nextId = NextId();

        if (nextId < _items.Count - 1)
            _items[nextId] = item;
        else
        {
            _items.Add(item);
            _generations.Add(0);
        }
        
        return new Handle
        {
            Id = nextId,
            Generation = _generations[nextId]
        };
    }

    /// <summary>
    /// Gets a reference to the stored item.
    /// This reference shouldn't be stored, as the underlaying array
    /// can resize at any time
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    public ref T Get(Handle handle)
    {
        // Marshalling has a little cost because .NET adds dumb checks, I should change this in the future
        return ref CollectionsMarshal.AsSpan(_items)[handle.Id];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Exists(Handle entity) => _generations[entity.Id] == entity.Generation;

    public bool Remove(Handle entity)
    {
        if (!Exists(entity)) return false;

        _generations[entity.Id]++;

        _deletedItems.Push(entity.Id);
        return true;
    }
}
