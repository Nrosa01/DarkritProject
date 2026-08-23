using System;
using System.Collections.Generic;
using System.Text;
using Darkrit.EntityModel;
using Darkrit.Physics.Boxy2D;

namespace Dakrit.Tests.EntityModel;

public class ComponentStoreTests
{
    ComponentStore<ComponentWithValueData> dataStore;
    ComponentStore<ComponentWithReferenceData> refStore;
    public ComponentStoreTests()
    {
        dataStore = new(5);
        refStore = new(5);
    }

    [Fact]
    public void Store_starts_empty()
    {
        Assert.Empty(dataStore);
        Assert.Empty(refStore);
    }
    
    [Fact]
    public void Store_is_empty_after_removing_last_component()
    {
        ref var component = ref dataStore.Add(new());
        Assert.NotEmpty(dataStore);
        dataStore.TryRemove(component.Handle);
        Assert.Empty(dataStore);
    }

    [Fact]
    public void Store_remove_component_modifies_its_fields()
    {
        ref var component = ref dataStore.Add(new());
        component.firstData = 17;
        dataStore.TryRemove(component.Handle);

        // Given Entity has a List<T> that is a reference type, when being removed, its set to
        // default to release that reference
        Assert.NotEqual(17, component.firstData);
    }

    [Fact]
    public void Store_remove_component_after_many_insertions_doesnt_modifies_its_fields()
    {
        ref var component = ref dataStore.Add(new());
        component.firstData = 17;

        for (int i = 0; i < 512; i++)
            dataStore.Add(new());

        dataStore.TryRemove(component.Handle);

        // Check previous test first
        // Given the array was resized, the entity that was removed by the handle
        // is not the same one we are referencing, thus its fields weren't overriden
        Assert.Equal(17, component.firstData);
    }


    // These componentes implement throw on the only method each one implements, if they throw, it's because they're implemneted

    [Fact]
    public void Drawble_component_draws()
    {
        EntityRegistry registry = new(5);
        registry.CreateEntity().AddComponent<DrawableComponent>();

        ComponentStore<DrawableComponent> store = registry.GetStore<DrawableComponent>();
        
        store.InitializePendingComponents();

        Assert.Throws<Exception>(() => store.Draw(default));

        store.Update(default);
        store.FixedUpdate(default);
    }

    [Fact]
    public void Updateable_component_updates()
    {
        EntityRegistry registry = new(5);
        registry.CreateEntity().AddComponent<UpdateableComponent>();

        ComponentStore<UpdateableComponent> store = registry.GetStore<UpdateableComponent>();

        store.InitializePendingComponents();

        Assert.Throws<Exception>(() => store.Update(default));

        store.Draw(default);
        store.FixedUpdate(default);
    }

    [Fact]
    public void FixedUpdateable_component_updates()
    {
        EntityRegistry registry = new(5);
        registry.CreateEntity().AddComponent<FixedUpdateableComponent>();

        ComponentStore<FixedUpdateableComponent> store = registry.GetStore<FixedUpdateableComponent>();

        store.InitializePendingComponents();

        Assert.Throws<Exception>(() => store.FixedUpdate(default));

        store.Draw(default);
        store.Update(default);
    }

    // When I add OnDestroy I will have to make sure to test it here
}
