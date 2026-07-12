using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Darkrit.EntityModel
{
    internal static class IdGenerator<T>
    {
        private static ulong s_nextId = 1;

        public static ulong Next()
        {
            ulong value = Interlocked.Increment(ref s_nextId);
            Debug.Assert(value != ulong.MaxValue);
            return value;
        }
    }

    public struct EntityWorldID : IEquatable<EntityWorldID>
    {
        public ulong Value { get; internal set; }

        public EntityWorldID(ulong value)
        {
            Debug.Assert(value != 0);
            Value = value;
        }

        public static EntityWorldID Generate() => new(IdGenerator<EntityWorldID>.Next());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Value = 0;

        public readonly bool Equals(EntityWorldID other) => Value == other.Value;

        public readonly bool IsValid => Value != 0;

        public static bool operator ==(EntityWorldID a, EntityWorldID b) => a.Value == b.Value;
        public static bool operator !=(EntityWorldID a, EntityWorldID b) => a.Value != b.Value;

        public readonly override int GetHashCode() => Value.GetHashCode();

        public readonly override bool Equals(object obj) => obj is EntityWorldID ID && Equals(ID);
    }

    public struct EntityID : IEquatable<EntityID>
    {
        public ulong Value { get; internal set; }

        public EntityID(ulong value)
        {
            Debug.Assert(value != 0);
            Value = value;
        }

        public static EntityID Generate() => new(IdGenerator<EntityID>.Next());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Value = 0;

        public readonly bool Equals(EntityID other) => Value == other.Value;

        public readonly bool IsValid => Value != 0;

        public static bool operator ==(EntityID a, EntityID b) => a.Value == b.Value;
        public static bool operator !=(EntityID a, EntityID b) => a.Value != b.Value;

        public readonly override int GetHashCode() => Value.GetHashCode();

        public readonly override bool Equals(object obj) => obj is EntityID ID && Equals(ID);
    }

    public struct ComponentID : IEquatable<ComponentID>
    {
        public ulong Value { get; internal set; }

        public ComponentID(ulong value)
        {
            Debug.Assert(value != 0);
            Value = value;
        }

        public static ComponentID Generate() => new(IdGenerator<ComponentID>.Next());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Value = 0;

        public readonly bool Equals(ComponentID other) => Value == other.Value;

        public readonly bool IsValid => Value != 0;

        public static bool operator ==(ComponentID a, ComponentID b) => a.Value == b.Value;
        public static bool operator !=(ComponentID a, ComponentID b) => a.Value != b.Value;

        public readonly override int GetHashCode() => Value.GetHashCode();

        public readonly override bool Equals(object obj) => obj is ComponentID ID && Equals(ID);
    }
}
