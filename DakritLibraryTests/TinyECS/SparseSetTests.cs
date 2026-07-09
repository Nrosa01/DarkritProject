using System;
using System.Collections.Generic;
using System.Text;
using MonoGameLibrary.TinyECS;

namespace DakritTests.TinyECS
{
    public class SparseSetTests
    {
        const int CAPACITY = 5;
        private readonly SparseSet set;

        public SparseSetTests()
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
        public void Add_AddsValueToSet()
        {
            set.Add(3);

            Assert.Equal(1, set.Count);
            Assert.True(set.Contains(3));
        }

        [Fact]
        public void Add_DuplicateValue_DoesNotIncreaseCount()
        {
            set.Add(3);
            set.Add(3);

            Assert.Equal(1, set.Count);
            Assert.True(set.Contains(3));
        }


        [Fact]
        public void Remove_ExistingValue_RemovesValue()
        {
            set.Add(3);

            set.Remove(3);

            Assert.Equal(0, set.Count);
            Assert.False(set.Contains(3));
        }

        [Fact]
        public void Remove_MiddleElement_KeepsSetConsistent()
        {
            set.Add(1);
            set.Add(2);
            set.Add(3);

            set.Remove(2);

            Assert.Equal(2, set.Count);
            Assert.True(set.Contains(1));
            Assert.False(set.Contains(2));
            Assert.True(set.Contains(3));
        }

        [Fact]
        public void Contains_ExistingValue_ReturnsTrue()
        {
            set.Add(4);

            Assert.True(set.Contains(4));
        }

        [Fact]
        public void Contains_RemovedValue_ReturnsFalse()
        {
            set.Add(4);
            set.Remove(4);

            Assert.False(set.Contains(4));
        }

        [Fact]
        public void Clear_RemovesAllValues()
        {
            set.Add(1);
            set.Add(2);
            set.Add(3);

            set.Clear();

            Assert.Equal(0, set.Count);
            Assert.Empty(set);
            Assert.False(set.Contains(1));
            Assert.False(set.Contains(2));
            Assert.False(set.Contains(3));
        }

        [Fact]
        public void GetEnumerator_ReturnsAllAddedValues()
        {
            set.Add(1);
            set.Add(3);
            set.Add(4);

            Assert.Equal([1, 3, 4], set);
        }
    }
}
