// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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