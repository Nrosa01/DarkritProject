// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Darkrit.EntityModel
{
    public abstract class SpatialComponent : Component, IDisposable
    {
        public abstract void Dispose();
    }
}
