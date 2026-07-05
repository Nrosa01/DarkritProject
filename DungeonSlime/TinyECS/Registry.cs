using System;
using System.Collections.Generic;
using System.Text;
using Entity = System.Int32;

namespace DungeonSlime.TinyECS
{
    public class World(int capacity)
    {
        private readonly int capactity = capacity;

        public void AddComponent<T>(Entity entity, T component)
        {
            throw new NotImplementedException();
        }

        public int Create()
        {
            throw new NotImplementedException();
        }

        public ref T GetComponent<T>(Entity entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Entity> View<T1, T2>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Entity> View<T1, T2, T3>()
        {
            throw new NotImplementedException();
        }
    }
}
