using System;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.Base;

/// <summary>
/// Agrupation of an item and its handle that represents it in a container
/// </summary>
/// <typeparam name="T"></typeparam>
public struct HandleItem<T> : IHandle<T> where T : new()
{
    public readonly static HandleItem<T> Default = new() { Handle = Handle<T>.Default, Item = default };

    public Handle<T> Handle { get; set; }

    public T Item;
}