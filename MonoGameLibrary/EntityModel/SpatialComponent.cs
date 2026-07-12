using System;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.EntityModel
{
    public abstract class SpatialComponent : Component, IDisposable
    {
        public abstract void Dispose();

        public enum Status
        {
            Unloaded = 0,
            Loading,
            Loaded,
            LoadingFailed,
            Initialized
        }
    }
}
