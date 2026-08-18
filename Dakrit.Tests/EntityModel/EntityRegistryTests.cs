using System;
using System.Collections.Generic;
using System.Text;
using Darkrit.EntityModel;

namespace Dakrit.Tests.EntityModel;

// From here, I will use this naming scheme: https://stackoverflow.com/questions/155436/unit-test-naming-best-practices
// Some old tests still use the 2009 naming scheme which I think it's still good but I prefer this one

public class EntityRegistryTests
{
    readonly EntityRegistry world;

    public EntityRegistryTests()
    {
        world = new(5);
    }

    [Fact]
    public void Worls_starts_empty()
    {
        Assert.Empty(world);
    }

    [Fact]
    public void World_is_empty_after_removing_last_entity()
    {
        var handle = world.CreateEntity();
        Assert.NotEmpty(world);
        world.RemoveEntity(handle);
        Assert.Empty(world);
    }

    [Fact]
    public void World_remove_entity_modify_its_fields()
    {
        var handle = world.CreateEntity();
        ref var entity = ref world.GetEntity(handle);
        entity.ActiveSelf = true;
        world.RemoveEntity(handle);

        // Given Entity has a List<T> that is a reference type, when being removed, its set to
        // default to release that reference
        Assert.False(entity.ActiveSelf);
    }

    [Fact]
    public void World_entities_get_reused()
    {
        var handle = world.CreateEntity();
        ref var entity = ref world.GetEntity(handle);
        entity.ActiveSelf = true;
        world.RemoveEntity(handle);
        handle = world.CreateEntity();
        Assert.Equal(1, world.Count);
        Assert.False(world.GetEntity(handle).ActiveSelf);
    }
}
