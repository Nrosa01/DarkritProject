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
        var handle = dataStore.Add(new());
        Assert.NotEmpty(dataStore);
        dataStore.TryRemove(handle);
        Assert.Empty(dataStore);
    }

    // These componentes implement throw on the only method each one implements, if they throw, it's because they're implemneted

    [Fact]
    public void Drawble_component_draws()
    {
        ComponentStore<DrawableComponent> store = new(5)
        {
            new()
        };
        Assert.Throws<Exception>(() => store.Draw(default));

        store.Update(default);
        store.FixedUpdate(default);
    }

    [Fact]
    public void Updateable_component_updates()
    {
        ComponentStore<UpdateableComponent> store = new(5)
        {
            new()
        };
        Assert.Throws<Exception>(() => store.Update(default));

        store.Draw(default);
        store.FixedUpdate(default);
    }

    [Fact]
    public void FixedUpdateable_component_updates()
    {
        ComponentStore<FixedUpdateableComponent> store = new(5)
        {
            new()
        };
        Assert.Throws<Exception>(() => store.FixedUpdate(default));

        store.Draw(default);
        store.Update(default);
    }

    // When I add OnDestroy I will have to make sure to test it here
}
