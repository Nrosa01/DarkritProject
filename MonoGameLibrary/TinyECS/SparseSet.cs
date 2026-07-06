using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MonoGameLibrary.TinyECS;

public class SparseSet : IEnumerable<int>
{
    private readonly int capacity;
    private int size;
    private readonly int[] dense;
    private readonly int[] sparse;

    public int Count => size;


    public SparseSet(int capacity)
    {
        this.capacity = capacity + 1;
        size = 0;
        dense = new int[capacity]; // Maybe here I should use List
        sparse = new int[capacity]; // Maybe should I use paginated views instead
    }

    public void Add(int value)
    {
        if (value >= 0 && value < capacity && !Contains(value))
        {
            sparse[value] = size;
            dense[size] = value;
            size++;
        }
    }

    public void Remove(int value)
    {
        if (Contains(value))
        {
            dense[sparse[value]] = dense[size - 1];
            sparse[dense[size - 1]] = sparse[value];
            size--;
        }
    }

    public bool Contains(int value)
    {
        if (value >= capacity || value < 0)
            return false;
        else
            return sparse[value] < size && dense[sparse[value]] == value;
    }

    public int Index(int value) => sparse[value];

    public void Clear() => size = 0;

    public IEnumerator<int> GetEnumerator()
    {
        var i = 0;
        while (i < size)
        {
            yield return dense[i];
            i++;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
