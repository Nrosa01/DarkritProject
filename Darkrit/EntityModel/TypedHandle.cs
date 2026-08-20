// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.Base;
using Handle = Darkrit.Base.Handle;

namespace Darkrit.EntityModel;

public struct TypedHandle
{
    public Handle handle;
    public int type;

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
