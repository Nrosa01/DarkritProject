using Darkrit.Base;
using Darkrit.DataStructures;

namespace DakritTests.DataStructures;

public class HandleMapGrowingTests
{
    const int CAPACITY = 5;
    private readonly HandleMapGrowing<int> set;

    public HandleMapGrowingTests()
    {
        set = new(CAPACITY);
    }
    
    [Fact]
    public void Constructor_InitializesEmptySet()
    {
        Assert.Equal(0, set.Count);
        Assert.Empty(set);
    }

    [Fact]
    public void Add_IncreasesCount()
    {
        var handle = set.Add(0);

        Assert.Equal(1, set.Count);
        Assert.True(set.IsValid(handle));
    }

    [Fact]
    public void Add_ReturnsDifferentHandles()
    {
        var handle1 = set.Add(0);
        var handle2 = set.Add(0);

        Assert.NotEqual(handle1, handle2);
        Assert.Equal(0, handle1.Generation);
        Assert.Equal(0, handle2.Generation);
    }

    [Fact]
    public void Add_GrowsBeyondInitialCapacity()
    {
        var handles = new List<Handle<int>>();

        for (var i = 0; i < CAPACITY + 5; i++)
        {
            handles.Add(set.Add(0));
        }

        Assert.Equal(CAPACITY + 5, set.Count);

        foreach (var handle in handles)
            Assert.True(set.IsValid(handle));
    }

    [Fact]
    public void ForeachRefHandleItem_ModifiesItem()
    {
        var set = new HandleMapGrowing<int>
        {
            1,
            2,
            3
        };

        foreach (ref var entry in set)
            entry.Item *= 10;

        var values = new List<int>();

        foreach (var entry in set)
            values.Add(entry.Item);

        Assert.Equal([10, 20, 30], values);
    }

    [Fact]
    public void Get_ReturnsStoredItem()
    {
        var item = 0;
        var handle = set.Add(item);

        ref var result = ref set.Get(handle);

        Assert.Equal(item, result);
    }

    [Fact]
    public void Get_ReturnsReferenceToStoredItem()
    {
        var handle = set.Add(0);

        ref var item = ref set.Get(handle);
        item = 0;

        Assert.Equal(item, set.Get(handle));
    }

    [Fact]
    public void IsValid_ReturnsFalseForDefaultHandle() => Assert.False(set.IsValid(default));

    [Fact]
    public void IsValid_ReturnsFalseForUnknownHandle()
    {
        var handle = new Handle<int>
        {
            Id = 1,
            Generation = 0
        };

        Assert.False(set.IsValid(handle));
    }

    [Fact]
    public void Remove_DecreasesCount()
    {
        var handle = set.Add(0);

        Assert.True(set.Remove(handle));
        Assert.Equal(0, set.Count);
    }

    [Fact]
    public void Remove_InvalidatesHandle()
    {
        var handle = set.Add(0);

        Assert.True(set.IsValid(handle));

        set.Remove(handle);

        Assert.False(set.IsValid(handle));
    }

    [Fact]
    public void Remove_ReturnsFalseForInvalidHandle()
    {
        Assert.False(set.Remove(default));
        Assert.Equal(0, set.Count);
    }

    [Fact]
    public void Remove_CalledTwice_ReturnsFalseSecondTime()
    {
        var handle = set.Add(0);

        Assert.True(set.Remove(handle));
        Assert.False(set.Remove(handle));

        Assert.Equal(0, set.Count);
    }

    [Fact]
    public void Add_AfterRemove_ReusesId()
    {
        var firstHandle = set.Add(0);
        set.Remove(firstHandle);

        var secondHandle = set.Add(0);

        Assert.Equal(firstHandle.Id, secondHandle.Id);
    }

    [Fact]
    public void Add_AfterRemove_IncrementsGeneration()
    {
        var firstHandle = set.Add(0);
        set.Remove(firstHandle);

        var secondHandle = set.Add(0);

        Assert.Equal(firstHandle.Id, secondHandle.Id);
        Assert.Equal(firstHandle.Generation + 1, secondHandle.Generation);
    }

    [Fact]
    public void Add_AfterRemove_InvalidatesOldHandle()
    {
        var firstHandle = set.Add(0);
        set.Remove(firstHandle);

        var secondHandle = set.Add(0);

        Assert.False(set.IsValid(firstHandle));
        Assert.True(set.IsValid(secondHandle));
    }

    [Fact]
    public void Iterate_VisitsAllValidItems()
    {
        var item1 = 0;
        var item2 = 0;
        var item3 = 0;

        set.Add(item1);
        set.Add(item2);
        set.Add(item3);

        var count = 0;

        foreach (var item in set)
        {
            count++;
            Assert.True(set.IsValid(item.Handle));
        }

        Assert.Equal(3, count);
    }

    [Fact]
    public void Iterate_DoesNotVisitRemovedItems()
    {
        var item1 = 0;
        var item2 = 0;
        var item3 = 0;

        var handle1 = set.Add(item1);
        set.Add(item2);
        set.Add(item3);

        set.Remove(handle1);

        var count = 0;

        foreach (var item in set)
        {
            count++;
            Assert.True(set.IsValid(item.Handle));
        }

        Assert.Equal(2, count);
    }

    [Fact]
    public void Items_ContainsInvalidFirstElement()
    {
        Assert.Equal(1, set.Items.Length);
        Assert.Equal(0, set.Items[0].Handle.Id);
    }

    [Fact]
    public void RemoveAndReuse_MultipleHandles_PreservesGenerations()
    {
        var handle1 = set.Add(0);
        var handle2 = set.Add(0);
        var handle3 = set.Add(0);

        set.Remove(handle2);
        set.Remove(handle1);

        var newHandle1 = set.Add(0);
        var newHandle2 = set.Add(0);

        Assert.Equal(handle1.Id, newHandle1.Id);
        Assert.Equal(handle2.Id, newHandle2.Id);

        Assert.Equal(handle1.Generation + 1, newHandle1.Generation);
        Assert.Equal(handle2.Generation + 1, newHandle2.Generation);

        Assert.False(set.IsValid(handle1));
        Assert.False(set.IsValid(handle2));
        Assert.True(set.IsValid(handle3));
        Assert.True(set.IsValid(newHandle1));
        Assert.True(set.IsValid(newHandle2));

        Assert.Equal(3, set.Count);
    }

    [Fact]
    public void Get_ReturnsReferenceThatCanModifyStoredItem()
    {
        var handle = set.Add(0);

        ref var item = ref set.Get(handle);
        item = 0;

        Assert.Equal(item, set.Get(handle));
    }

    [Fact]
    public void Get_ReturnsReferenceToSameStoredItem()
    {
        var original = 0;
        var handle = set.Add(original);

        ref var item = ref set.Get(handle);

        Assert.Equal(original, item);
    }

    [Fact]
    public void IsValid_ReturnsFalseForNegativeId()
    {
        var handle = new Handle<int>
        {
            Id = -1,
            Generation = 0
        };

        Assert.False(set.IsValid(handle));
    }

    [Fact]
    public void IsValid_ReturnsFalseForIdOutsideRange()
    {
        var handle = new Handle<int>
        {
            Id = 1,
            Generation = 0
        };

        Assert.False(set.IsValid(handle));

        set.Add(0);

        var outOfRangeHandle = new Handle<int>
        {
            Id = 2,
            Generation = 0
        };

        Assert.False(set.IsValid(outOfRangeHandle));
    }
}
