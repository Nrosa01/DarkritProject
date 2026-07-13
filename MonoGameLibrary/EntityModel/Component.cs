// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.EntityModel
{
    public class Component
    {
        protected ComponentID componentID = ComponentID.Generate();

        public EntityID EntityID { get;  protected set; }

        protected Status status = Status.Unloaded;

        protected bool isRegisteredWithEntity = false;

        protected bool isRegisteredWithWorld = false;

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
