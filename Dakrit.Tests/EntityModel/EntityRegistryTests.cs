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
        var handle = world.CreateEntityByHandle();
        Assert.NotEmpty(world);
        world.RemoveEntity(handle);
        Assert.Empty(world);
    }

    [Fact]
    public void World_remove_entity_modifies_its_fields()
    {
        var handle = world.CreateEntityByHandle();
        ref var entity = ref world.GetEntity(handle);
        entity.ActiveSelf = true;
        world.RemoveEntity(handle);

        // Given Entity has a List<T> that is a reference type, when being removed, its set to
        // default to release that reference
        Assert.False(entity.ActiveSelf);
    }

    [Fact]
    public void World_remove_entity_after_many_insertions_doesnt_modifies_its_fields()
    {
        var handle = world.CreateEntityByHandle();
        ref var entity = ref world.GetEntity(handle);
        entity.ActiveSelf = true;

        // Creating so many entities, the array will have to resize many times
        for (int i = 0; i < 512; i++)
            world.CreateEntityByHandle();

        world.RemoveEntity(handle);

        // Check previous test first
        // Given the array was resized, the entity that was removed by the handle
        // is not the same one we are referencing, thus its fields weren't overriden
        Assert.True(entity.ActiveSelf);
    }


    [Fact]
    public void World_entities_get_reused()
    {
        var handle = world.CreateEntityByHandle();
        ref var entity = ref world.GetEntity(handle);
        entity.ActiveSelf = false;
        world.RemoveEntity(handle);
        handle = world.CreateEntityByHandle();
        Assert.Equal(1, world.Count);
        Assert.True(world.GetEntity(handle).ActiveSelf);
    }

    [Fact]
    public void Entity_out_is_a_ref()
    {
        var handle = world.CreateEntityByHandle();
        ref Entity entity = ref world.GetEntity(handle);
        ref Entity reference = ref world.GetEntity(handle);
        Assert.True(reference.ActiveSelf);
        entity.ActiveSelf = false;
        Assert.False(reference.ActiveSelf);
    }
}
