// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// The following code was adapted from: https://github.com/BobbyAnguelov/Esoterica/blob/main/Code/Engine/Entity/EntityComponent.h

using System.Diagnostics;
using Darkrit.Base;
using Microsoft.Xna.Framework.Content;

namespace Darkrit.EntityModel
{
    public abstract class Component
    {
        public enum Status
        {
            Unloaded = 0,
            Loading,
            Loaded,
            LoadingFailed,
            Initialized
        }

        // The unique ID for this component
        protected ComponentID componentID = ComponentID.Generate();

        // The ID of the entity that owns this component
        public EntityID EntityID { get;  protected set; }

        // The name of the component
        public StringID NameID { get; protected set; }

        // Component status
        public Status CurrentStatus { get; protected set; } = Status.Unloaded;

        public bool HasLoadingFailed => CurrentStatus == Status.LoadingFailed;
        public bool IsUnloaded => CurrentStatus == Status.Unloaded;
        public bool IsLoading => CurrentStatus == Status.Loading;
        public bool IsLoaded => CurrentStatus == Status.Loaded;
        public bool IsInitialized => CurrentStatus == Status.Initialized;


        // Registered with its parent entity's local systems
        protected bool isRegisteredWithEntity = false;
        // Registered with the global systems in it's parent world
        protected bool isRegisteredWithWorld = false;

        // Do we allow multiple components of the same type per entity?
        public virtual bool IsSingletonComponent => true;

        // Request load of all component data - loading takes time
        protected abstract void Load(ContentManager contentManager);

        // Update loading state, this will check all dependencies
        protected abstract void UpdateLoading();

        // Request unload of component data, unloading is instant
        protected abstract void Unload(ContentManager contentManager);

        // Called when an component finishes loading all its resources
        // Note: this is only called if the loading succeeds and you are guaranteed all resources to be valid and so should assert on that
        internal void InternalInitialize()
        {
            Debug.Assert(EntityID.IsValid && CurrentStatus == Status.Loaded);
            CurrentStatus = Status.Initialized;
            Initialize();
        }

        protected abstract void Initialize();

        // Called just before a component begins unloading
        internal void InternalShutdown()
        {
            Debug.Assert(EntityID.IsValid && CurrentStatus == Status.Initialized);
            Shutdown();
        }

        protected abstract void Shutdown();
    }
}
