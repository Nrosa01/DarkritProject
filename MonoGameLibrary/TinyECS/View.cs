using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using Entity = System.Int32;

namespace MonoGameLibrary.TinyECS;

public struct View<T>(Registry World) : IEnumerable<Entity>
{
    public readonly IEnumerator<Entity> GetEnumerator() => World.Assure<T>().Set.GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public struct View<T, U>(Registry World) : IEnumerable<Entity>
{
    public readonly IEnumerator<Entity> GetEnumerator()
    {
        var store2 = World.Assure<U>();
        foreach (var entity in World.Assure<T>().Set)
        {
            if (!store2.Contains(entity)) continue;
            yield return entity;
        }
    }

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public struct View<T, U, V>(Registry World) : IEnumerable<Entity>
{
    public readonly IEnumerator<Entity> GetEnumerator()
    {
        var store2 = World.Assure<U>();
        var store3 = World.Assure<V>();
        foreach (var entity in World.Assure<T>().Set)
        {
            if (!store2.Contains(entity) || !store3.Contains(entity)) continue;
            yield return entity;
        }
    }

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
