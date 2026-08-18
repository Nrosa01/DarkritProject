using System;
using System.Collections.Generic;
using System.Text;
using Darkrit.Base;
using Darkrit.EntityModel;

namespace Dakrit.Tests.EntityModel;

public class EntityTests
{
    readonly EntityRegistry world;
    Handle<Entity> entityHandle;

    public ref Entity Entity => ref world.GetEntity(entityHandle);

    public EntityTests()
    {
        world = new();
        entityHandle = world.CreateEntity();
    }

    public void Can_remove_component()
    {
        // Normal
        Entity.AddComponent<ComponentA>();
        var componentHandle = Entity.GetComponentHandle<ComponentA>();
        Assert.Equal(1, componentHandle.Id);
        Entity.RemoveComponent<ComponentA>();
        componentHandle = Entity.GetComponentHandle<ComponentA>();
        Assert.Equal(0, componentHandle.Id); // O means invalid

        // Using GetComponent
        Entity.AddComponent<ComponentA>();
        Entity.GetComponentHandle<ComponentA>();
        Assert.Equal(1, componentHandle.Id);
        Assert.Equal(1, componentHandle.Generation);
        Entity.RemoveComponent<ComponentA>();
        Assert.False(Entity.TryGetComponent<ComponentA>(out var A));
    }

    public void TryGetComponent_returns_a_reference()
    {
        Entity.AddComponent(new ComponentWithValueData()
        {
            firstData = 13
        });

        Entity.TryGetComponent<ComponentWithValueData>(out var data);
        Assert.Equal(13, data.firstData);
    }

    public void Can_add_two_components_of_same_type()
    {
        var handle1 = Entity.AddComponent<ComponentWithValueData>();
        var handle2 = Entity.AddComponent<ComponentWithValueData>();

        ref var ref1 = ref Entity.GetComponent(handle1);
        ref var ref2 = ref Entity.GetComponent(handle2);

        ref1.firstData = 4;
        ref2.firstData = 7;

        Assert.Equal(4, ref1.firstData);
        Assert.Equal(7, ref2.firstData);
    }

    public void Parameterless_get_component_removes_first_ocurrence()
    {
        var handle1 = Entity.AddComponent<ComponentA>();
        var handle2 = Entity.AddComponent<ComponentA>();

        Entity.RemoveComponent<ComponentA>();
        Assert.True(Entity.HasComponent<ComponentA>());
        Assert.True(Entity.HasComponent<ComponentA>(handle2));
        Assert.False(Entity.HasComponent<ComponentA>(handle1));
    }
}
