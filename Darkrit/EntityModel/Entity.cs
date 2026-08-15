// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.Base;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Darkrit.EntityModel;

public record struct EntityId(Int32 Id, Int32 Generation);

public struct Entity
{
    public EntityId Id { get; init; }

    public EntityRegistry World { get; init; }

    readonly Dictionary<int, Handle<IComponent>> componentIds = [];

    /// <summary>
    /// Whether this Entity is active in the hierarchy
    /// </summary>
    public bool ActiveSelf { get; set; }

    /// <summary>
    /// Whether this Entity is active in the scene
    /// Say, Entity A has a child Entity B. B could be active but A 
    /// be inactive, which would result in B <see cref="ActiveInHierachy"/> be false
    /// while <see cref="ActiveSelf"/> is true
    /// </summary>
    public bool ActiveInHierachy { get; internal set; }

    public Entity()
    {
    }

    public Handle<T> GetComponentHandle<T>() where T : struct, IComponent
    {
        if (componentIds[ComponentTypeId<T>.Id] is Handle<T> handle)
            return handle;

        throw new InvalidOperationException();
    }

    public ref T GetComponent<T>() where T : struct, IComponent
    {
       return ref World.GetComponent<T>(GetComponentHandle<T>());
    }
}
