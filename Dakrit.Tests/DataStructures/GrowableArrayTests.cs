using Darkrit.DataStructures;

namespace DakritTests.DataStructures;

public class GrowableArrayTests
{
    const int CAPACITY = 5;
    private readonly GrowableArray<int> set;

    public GrowableArrayTests() => set = new(CAPACITY);

    [Fact]
    public void Constructor_SetsInitialCapacity()
    {
        var array = new GrowableArray<int>(CAPACITY);

        Assert.Equal(CAPACITY, array.Capacity);
        Assert.Equal(0, array.Count);
    }

    [Fact]
    public void Constructor_WithZeroCapacity_StartsEmpty()
    {
        var array = new GrowableArray<int>(0);

        Assert.Equal(0, array.Capacity);
        Assert.Equal(0, array.Count);
    }

    [Fact]
    public void Constructor_WithNegativeCapacity_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => new GrowableArray<int>(-1));

    [Fact]
    public void Add_IncreasesCount()
    {
        set.Add(10);

        Assert.Equal(1, set.Count);
    }

    [Fact]
    public void Add_StoresItem()
    {
        set.Add(42);

        Assert.Equal(42, set[0]);
    }

    [Fact]
    public void Add_StoresItemsInOrder()
    {
        set.Add(10);
        set.Add(20);
        set.Add(30);

        Assert.Equal(10, set[0]);
        Assert.Equal(20, set[1]);
        Assert.Equal(30, set[2]);
    }

    [Fact]
    public void Add_UpToCapacity_DoesNotGrow()
    {
        for (var i = 0; i < CAPACITY; i++)
            set.Add(i);

        Assert.Equal(CAPACITY, set.Count);
        Assert.Equal(CAPACITY, set.Capacity);
    }

    [Fact]
    public void Add_WhenCapacityIsReached_GrowsArray()
    {
        for (var i = 0; i < CAPACITY + 1; i++)
            set.Add(i);

        Assert.Equal(CAPACITY + 1, set.Count);
        Assert.Equal(CAPACITY * 2, set.Capacity);
    }

    [Fact]
    public void Add_WhenCapacityIsZero_GrowsToFour()
    {
        var array = new GrowableArray<int>(0)
        {
            42
        };

        Assert.Equal(1, array.Count);
        Assert.Equal(4, array.Capacity);
        Assert.Equal(42, array[0]);
    }

    [Fact]
    public void Grow_PreservesExistingItems()
    {
        for (var i = 0; i < CAPACITY; i++)
            set.Add(i);

        set.Add(99);

        for (var i = 0; i < CAPACITY; i++)
            Assert.Equal(i, set[i]);

        Assert.Equal(99, set[CAPACITY]);
    }

    [Fact]
    public void Grow_DoublesCapacity()
    {
        for (var i = 0; i < CAPACITY; i++)
            set.Add(i);

        set.Add(5);

        Assert.Equal(10, set.Capacity);

        for (var i = 6; i < 11; i++)
            set.Add(i);

        Assert.Equal(20, set.Capacity);
    }

    [Fact]
    public void Indexer_CanReadValue()
    {
        set.Add(123);

        Assert.Equal(123, set[0]);
    }

    [Fact]
    public void Indexer_CanModifyValue()
    {
        set.Add(123);

        set[0] = 456;

        Assert.Equal(456, set[0]);
    }

    [Fact]
    public void Indexer_ReturnsReference()
    {
        set.Add(123);

        ref var value = ref set[0];
        value = 456;

        Assert.Equal(456, set[0]);
    }

    [Fact]
    public void AsReadOnlySpan_ReturnsOnlyStoredItems()
    {
        set.Add(10);
        set.Add(20);
        set.Add(30);

        var span = set.AsReadOnlySpan();

        Assert.Equal(3, span.Length);
        Assert.Equal(10, span[0]);
        Assert.Equal(20, span[1]);
        Assert.Equal(30, span[2]);
    }

    [Fact]
    public void AsReadOnlySpan_DoesNotIncludeUnusedCapacity()
    {
        set.Add(10);
        set.Add(20);

        var span = set.AsReadOnlySpan();

        Assert.Equal(set.Count, span.Length);
        Assert.Equal(2, span.Length);
    }

    [Fact]
    public void AsReadOnlySpan_EmptyArray_ReturnsEmptySpan()
    {
        var array = new GrowableArray<int>(CAPACITY);

        var span = array.AsReadOnlySpan();

        Assert.True(span.IsEmpty);
        Assert.Equal(0, span.Length);
    }

    [Fact]
    public void Enumeration_ReturnsAllItemsInOrder()
    {
        set.Add(10);
        set.Add(20);
        set.Add(30);

        var items = set.ToArray();

        Assert.Equal([10, 20, 30], items);
    }

    [Fact]
    public void Enumeration_DoesNotReturnUnusedCapacity()
    {
        set.Add(10);
        set.Add(20);

        var items = set.ToArray();

        Assert.Equal(2, items.Length);
        Assert.Equal([10, 20], items);
    }

    [Fact]
    public void Foreach_EnumeratesItems()
    {
        set.Add(10);
        set.Add(20);
        set.Add(30);

        var result = new List<int>();

        foreach (var item in set)
            result.Add(item);

        Assert.Equal([10, 20, 30], result);
    }

    [Fact]
    public void GenericEnumerator_ReturnsExpectedItems()
    {
        set.Add(10);
        set.Add(20);

        using var enumerator = set.GetEnumerator();

        Assert.True(enumerator.MoveNext());
        Assert.Equal(10, enumerator.Current);

        Assert.True(enumerator.MoveNext());
        Assert.Equal(20, enumerator.Current);

        Assert.False(enumerator.MoveNext());
    }

    [Fact]
    public void NonGenericEnumerator_ReturnsExpectedItems()
    {
        set.Add(10);
        set.Add(20);

        var enumerable = (System.Collections.IEnumerable)set;
        var enumerator = enumerable.GetEnumerator();

        Assert.True(enumerator.MoveNext());
        Assert.Equal(10, enumerator.Current);

        Assert.True(enumerator.MoveNext());
        Assert.Equal(20, enumerator.Current);

        Assert.False(enumerator.MoveNext());
    }

    [Fact]
    public void Add_MultipleGrowths_PreservesAllItems()
    {
        const int count = 100;

        for (var i = 0; i < count; i++)
            set.Add(i);

        Assert.Equal(count, set.Count);

        for (var i = 0; i < count; i++)
            Assert.Equal(i, set[i]);
    }

    [Fact]
    public void Count_AlwaysMatchesNumberOfAddedItems()
    {
        for (var i = 0; i < 50; i++)
        {
            set.Add(i);

            Assert.Equal(i + 1, set.Count);
        }
    }
}