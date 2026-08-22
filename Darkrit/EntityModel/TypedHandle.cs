// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.Base;
using Handle = Darkrit.Base.Handle;

namespace Darkrit.EntityModel;

/// <summary>
/// Used when I want to store handles of many component types in a non generic collection
/// </summary>
public struct TypedHandle
{
    /// <summary>
    /// 
    /// </summary>
    public Handle handle;
    
    /// <summary>
    /// Type of the component, see <see cref="ComponentTypeId{T}"/>
    /// </summary>
    public int type;

    /// <summary>
    /// Creates a TypedHandle from a generic Handle
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="handle"></param>
    /// <returns></returns>
    public static TypedHandle Create<T>(Handle<T> handle) where T : struct, IComponent => new()
    {
        handle = new Handle
        {
            Id = handle.Id,
            Generation = handle.Generation
        },
        type = ComponentTypeId<T>.Id
    };
}
