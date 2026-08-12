using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.Base;

public readonly struct Handle<T> : IEquatable<Handle<T>>
{
    public int Id { get; init; }
    public int Generation { get; init; }

    public readonly static Handle<T> Default = new() { Id = 0, Generation = 0 };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Handle<T> other) => Id == other.Id && Generation == other.Generation;

    public static bool operator ==(Handle<T> left, Handle<T> right) => left.Equals(right);

    public static bool operator !=(Handle<T> left, Handle<T> right) => !left.Equals(right);

    public override bool Equals(object obj) => obj is Handle<T> handle && Equals(handle);

    public override int GetHashCode() => HashCode.Combine(Id, Generation);
}


public interface IHandle<T>
{
    public Handle<T> Handle { get; set; }
}