using Darkrit.DataStructures;

namespace DakritTests.DataStructures;

public class RingBufferTests
{
    const int CAPACITY = 5;
    private readonly RingBuffer<int> buffer;

    public RingBufferTests()
    {
        buffer = new(CAPACITY);
    }

    [Fact]
    public void RingBuffer_GetEnumeratorConstructorCapacity_ReturnsEmptyCollection()
    {
        var buffer = new RingBuffer<string>(5);
        Assert.True(buffer.IsEmpty);
    }


    [Fact]
    public void RingBuffer_ConstructorSizeIndexAccess_CorrectContent()
    {
        var buffer = new RingBuffer<int>(5, [0, 1, 2, 3]);

        Assert.Equal(5, buffer.Capacity);
        Assert.Equal(4, buffer.Size);
        for (int i = 0; i < 4; i++)
        {
            Assert.Equal(i, buffer[i]);
        }
    }

    [Fact]
    public void RingBuffer_Constructor_ExceptionWhenSourceIsLargerThanCapacity()
    {
        Assert.Throws<ArgumentException>(() => new RingBuffer<int>(3, [0, 1, 2, 3]));
    }

    [Fact]
    public void RingBuffer_GetEnumeratorConstructorDefinedArray_CorrectContent()
    {
        var buffer = new RingBuffer<int>(5, [0, 1, 2, 3]);

        int x = 0;
        foreach (var item in buffer)
        {
            Assert.Equal(x, item);
            x++;
        }
    }

    [Fact]
    public void RingBuffer_PushBack_CorrectContent()
    {
        var buffer = new RingBuffer<int>(5);

        for (int i = 0; i < 5; i++)
        {
            buffer.PushBack(i);
        }

        Assert.Equal(0, buffer.Front());
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(i, buffer[i]);
        }
    }

    [Fact]
    public void RingBuffer_PushBackOverflowingBuffer_CorrectContent()
    {
        var buffer = new RingBuffer<int>(5);

        for (int i = 0; i < 10; i++)
        {
            buffer.PushBack(i);
        }

        Assert.Equal([5, 6, 7, 8, 9], buffer.ToArray());
    }

    [Fact]
    public void RingBuffer_GetEnumeratorOverflowedArray_CorrectContent()
    {
        var buffer = new RingBuffer<int>(5);

        for (int i = 0; i < 10; i++)
        {
            buffer.PushBack(i);
        }

        // buffer should have [5,6,7,8,9]
        int x = 5;
        foreach (var item in buffer)
        {
            Assert.Equal(x, item);
            x++;
        }
    }

    [Fact]
    public void RingBuffer_ToArrayConstructorDefinedArray_CorrectContent()
    {
        var buffer = new RingBuffer<int>(5, [0, 1, 2, 3]);

        Assert.Equal([0, 1, 2, 3], buffer.ToArray());
    }

    [Fact]
    public void RingBuffer_ToArrayOverflowedBuffer_CorrectContent()
    {
        var buffer = new RingBuffer<int>(5);

        for (int i = 0; i < 10; i++)
        {
            buffer.PushBack(i);
        }

        Assert.Equal([5, 6, 7, 8, 9], buffer.ToArray());
    }

    [Fact]
    public void RingBuffer_PushFront_CorrectContent()
    {
        var buffer = new RingBuffer<int>(5);

        for (int i = 0; i < 5; i++)
        {
            buffer.PushFront(i);
        }

        Assert.Equal([4, 3, 2, 1, 0], buffer.ToArray());
    }

    [Fact]
    public void RingBuffer_PushFrontAndOverflow_CorrectContent()
    {
        var buffer = new RingBuffer<int>(5);

        for (int i = 0; i < 10; i++)
        {
            buffer.PushFront(i);
        }

        Assert.Equal([9, 8, 7, 6, 5], buffer.ToArray());
    }

    [Fact]
    public void RingBuffer_Front_CorrectItem()
    {
        var buffer = new RingBuffer<int>(5, [0, 1, 2, 3, 4]);

        Assert.Equal(0, buffer.Front());
    }

    [Fact]
    public void RingBuffer_Back_CorrectItem()
    {
        var buffer = new RingBuffer<int>(5, [0, 1, 2, 3, 4]);
        Assert.Equal(4, buffer.Back());
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(6)]
    public void RingBuffer_BackOfBufferOverflowByOne_CorrectItem(int value)
    {
        var buffer = new RingBuffer<int>(5, [0, 1, 2, 3, 4]);
        buffer.PushBack(value);
        var newBuffer = buffer.ToArray();
        Assert.Equal([1, 2, 3, 4, value], [.. buffer]);
        Assert.Equal(value, buffer.Back());
    }

    [Fact]
    public void RingBuffer_Front_EmptyBufferThrowsException()
    {
        var buffer = new RingBuffer<int>(5);

        Assert.Throws<InvalidOperationException>(() => buffer.Front());
    }

    [Fact]
    public void RingBuffer_Back_EmptyBufferThrowsException()
    {
        var buffer = new RingBuffer<int>(5);
        Assert.Throws<InvalidOperationException>(() => buffer.Back());
    }

    [Fact]
    public void RingBuffer_PopBack_RemovesBackElement()
    {
        var buffer = new RingBuffer<int>(5, [0, 1, 2, 3, 4]);

        Assert.Equal(5, buffer.Size);

        buffer.PopBack();

        Assert.Equal(4, buffer.Size);
        Assert.Equal([0, 1, 2, 3], [.. buffer]);
    }

    [Fact]
    public void RingBuffer_PopBackInOverflowBuffer_RemovesBackElement()
    {
        var buffer = new RingBuffer<int>(5, [0, 1, 2, 3, 4]);
        buffer.PushBack(5);

        Assert.Equal(5, buffer.Size);
        Assert.Equal([1, 2, 3, 4, 5], buffer.ToArray());

        buffer.PopBack();

        Assert.Equal(4, buffer.Size);
        Assert.Equal([1, 2, 3, 4], buffer.ToArray());
    }

    [Fact]
    public void RingBuffer_PopFront_RemovesBackElement()
    {
        var buffer = new RingBuffer<int>(5, [0, 1, 2, 3, 4]);

        Assert.Equal(5, buffer.Size);

        buffer.PopFront();

        Assert.Equal(4, buffer.Size);
        Assert.Equal([1, 2, 3, 4], buffer.ToArray());
    }

    [Fact]
    public void RingBuffer_PopFrontInOverflowBuffer_RemovesBackElement()
    {
        var buffer = new RingBuffer<int>(5, [0, 1, 2, 3, 4]);
        buffer.PushFront(5);

        Assert.Equal(5, buffer.Size);
        Assert.Equal([5, 0, 1, 2, 3], buffer.ToArray());

        buffer.PopFront();

        Assert.Equal(4, buffer.Size);
        Assert.Equal([0, 1, 2, 3], buffer.ToArray());
    }

    [Fact]
    public void RingBuffer_SetIndex_ReplacesElement()
    {
        var buffer = new RingBuffer<int>(5, [0, 1, 2, 3, 4]);

        buffer[1] = 10;
        buffer[3] = 30;

        Assert.Equal([0, 10, 2, 30, 4], buffer.ToArray());
    }

    [Fact]
    public void RingBuffer_WithDifferentSizeAndCapacity_BackReturnsLastArrayPosition()
    {
        var buffer = new RingBuffer<int>(5, [0, 1, 2, 3, 4]);

        buffer.PopFront(); // (make size and capacity different)

        Assert.Equal(4, buffer.Back());
    }

    [Fact]
    public void RingBuffer_Clear_ClearsContent()
    {
        var buffer = new RingBuffer<int>(5, [4, 3, 2, 1, 0]);

        buffer.Clear();

        Assert.Equal(0, buffer.Size);
        Assert.Equal(5, buffer.Capacity);
        Assert.Equal([], buffer.ToArray());
    }

    [Fact]
    public void RingBuffer_Clear_WorksNormallyAfterClear()
    {
        var buffer = new RingBuffer<int>(5, [4, 3, 2, 1, 0]);

        buffer.Clear();
        for (int i = 0; i < 5; i++)
            buffer.PushBack(i);

        Assert.Equal(0, buffer.Front());
        for (int i = 0; i < 5; i++)
            Assert.Equal(i, buffer[i]);
    }
}
