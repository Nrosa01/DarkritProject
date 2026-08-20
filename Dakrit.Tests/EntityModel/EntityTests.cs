using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
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
        entityHandle = world.CreateEntityByHandle();
    }

    [Fact]
    public void Can_remove_component_by_handle()
    {
        Entity.AddComponent<ComponentA>();
        var componentHandle = Entity.GetComponentHandle<ComponentA>();
        Assert.Equal(1, componentHandle.Id);
        Entity.RemoveComponent<ComponentA>();
        Assert.False(Entity.HasComponent<ComponentA>(componentHandle));
    }

    [Fact]
    public void Can_remove_component_by_generics()
    {
        // Using GetComponent
        Entity.AddComponent<ComponentA>();
        Entity.RemoveComponent<ComponentA>();
        Assert.False(Entity.HasComponent<ComponentA>());
    }

    [Fact]
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

    [Fact]
    public void Parameterless_get_component_removes_first_ocurrence()
    {
        var handle1 = Entity.AddComponent<ComponentA>();
        var handle2 = Entity.AddComponent<ComponentA>();

        Entity.RemoveComponent<ComponentA>();
        Assert.True(Entity.HasComponent<ComponentA>());
        Assert.True(Entity.HasComponent<ComponentA>(handle2));
        Assert.False(Entity.HasComponent<ComponentA>(handle1));
    }

    ////////////////////////////
    //// ✨ Hiearchies ✨//////
    ////////////////////////////

    [Fact]
    public void Entity_starts_without_parent()
    {
        var handle = world.CreateEntityByHandle();
        ref var entity = ref world.GetEntity(handle);
        Assert.Equal(0, entity.ChildCount);
        Assert.Equal(0, entity._parent.Id);
        Assert.Equal(0, entity._firstChild.Id);
        Assert.Equal(0, entity._nextSibling.Id);
        Assert.Equal(0, entity._previousSibling.Id);
    }

    [Fact]
    public void Can_set_parent()
    {
        var parent = world.CreateEntityByHandle();
        var child = world.CreateEntityByHandle(parent);

        Assert.Equal(parent, world.GetEntity(child)._parent);
        Assert.Equal(child, world.GetEntity(parent)._firstChild);
        Assert.Equal(1, world.GetEntity(parent).ChildCount);
    }

    [Fact]
    public void Can_set_multiple_children()
    {
        var parent = world.CreateEntityByHandle();
        var child1 = world.CreateEntityByHandle();
        var child2 = world.CreateEntityByHandle();
        var child3 = world.CreateEntityByHandle();

        world.GetEntity(child1).TrySetParentFirst(parent);
        world.GetEntity(child2).TrySetParentFirst(parent);
        world.GetEntity(child3).TrySetParentFirst(parent);

        ref var p = ref world.GetEntity(parent);
        
        Assert.Equal(3, p.ChildCount);

        Assert.Equal(child3, p._firstChild);

        Assert.Equal(child3, world.GetEntity(child2)._previousSibling);
        Assert.Equal(child1, world.GetEntity(child2)._nextSibling);

        Assert.Equal(child2, world.GetEntity(child1)._previousSibling);
        Assert.Equal(0, world.GetEntity(child1)._nextSibling.Id);
    }

    [Fact]
    public void Can_set_multiple_children_last()
    {
        var parent = world.CreateEntityByHandle();
        var child1 = world.CreateEntityByHandle();
        var child2 = world.CreateEntityByHandle();
        var child3 = world.CreateEntityByHandle();

        world.GetEntity(child1).TrySetParent(parent);
        world.GetEntity(child2).TrySetParent(parent);
        world.GetEntity(child3).TrySetParent(parent);

        ref var p = ref world.GetEntity(parent);

        Assert.Equal(3, p.ChildCount);

        Assert.Equal(child1, p._firstChild);

        Assert.Equal(0, world.GetEntity(child1)._previousSibling.Id);
        Assert.Equal(child2, world.GetEntity(child1)._nextSibling);

        Assert.Equal(child1, world.GetEntity(child2)._previousSibling);
        Assert.Equal(child3, world.GetEntity(child2)._nextSibling);

        Assert.Equal(child2, world.GetEntity(child3)._previousSibling);
        Assert.Equal(0, world.GetEntity(child3)._nextSibling.Id);
    }

    [Fact]
    public void Can_add_child()
    {
        var parent = world.CreateEntityByHandle();
        var child = world.CreateEntityByHandle();

        Assert.True(world.GetEntity(parent).TryAddChild(child));

        Assert.Equal(parent, world.GetEntity(child)._parent);
        Assert.Equal(child, world.GetEntity(parent)._firstChild);
        Assert.Equal(1, world.GetEntity(parent).ChildCount);
    }

    [Fact]
    public void Can_add_multiple_children_in_order()
    {
        var parent = world.CreateEntityByHandle();
        var child1 = world.CreateEntityByHandle();
        var child2 = world.CreateEntityByHandle();
        var child3 = world.CreateEntityByHandle();

        world.GetEntity(parent).TryAddChild(child1);
        world.GetEntity(parent).TryAddChild(child2);
        world.GetEntity(parent).TryAddChild(child3);

        Assert.Equal(3, world.GetEntity(parent).ChildCount);

        var children = new List<Handle<Entity>>();

        foreach (var child in world.GetEntity(parent).Children)
            children.Add(child.Handle);

        Assert.Equal([child1, child2, child3], children);
    }

    [Fact]
    public void Can_set_sibling_index()
    {
        var parent = world.CreateEntityByHandle();
        var child1 = world.CreateEntityByHandle();
        var child2 = world.CreateEntityByHandle();
        var child3 = world.CreateEntityByHandle();

        world.GetEntity(child1).TrySetParent(parent);
        world.GetEntity(child2).TrySetParent(parent);
        world.GetEntity(child3).TrySetParent(parent);

        Assert.Equal(3, world.GetEntity(parent).ChildCount);

        Assert.True(world.GetEntity(child3).TrySetSiblingIndex(0));

        Assert.Equal(3, world.GetEntity(parent).ChildCount);

        var children = new List<Handle<Entity>>();

        foreach (var child in world.GetEntity(parent).Children)
            children.Add(child.Handle);

        Assert.Equal([child3, child1, child2], children);

        Assert.Equal(0, world.GetEntity(child3)._previousSibling.Id);
        Assert.Equal(child1, world.GetEntity(child3)._nextSibling);

        Assert.Equal(child3, world.GetEntity(child1)._previousSibling);
        Assert.Equal(child2, world.GetEntity(child1)._nextSibling);

        Assert.Equal(child1, world.GetEntity(child2)._previousSibling);
        Assert.Equal(0, world.GetEntity(child2)._nextSibling.Id);
    }

    [Fact]
    public void Can_move_sibling_to_last_index()
    {
        var parent = world.CreateEntityByHandle();
        var child1 = world.CreateEntityByHandle();
        var child2 = world.CreateEntityByHandle();
        var child3 = world.CreateEntityByHandle();

        world.GetEntity(child1).TrySetParent(parent);
        world.GetEntity(child2).TrySetParent(parent);
        world.GetEntity(child3).TrySetParent(parent);

        Assert.Equal(3, world.GetEntity(parent).ChildCount);

        Assert.True(world.GetEntity(child1).TrySetSiblingIndex(2));

        Assert.Equal(3, world.GetEntity(parent).ChildCount);

        var children = new List<Handle<Entity>>();

        foreach (var child in world.GetEntity(parent).Children)
            children.Add(child.Handle);

        Assert.Equal([child2, child3, child1], children);
    }

    [Fact]
    public void Destroy_entity_also_destroys_children()
    {
        var parent = world.CreateEntityByHandle();
        var child = world.CreateEntityByHandle();
        var grandChild = world.CreateEntityByHandle();

        world.GetEntity(child).TrySetParentFirst(parent);
        world.GetEntity(grandChild).TrySetParentFirst(child);

        Assert.True(world.RemoveEntity(parent));

        Assert.Equal(0, world.GetEntityReadonly(parent).Handle.Id);
        Assert.Equal(0, world.GetEntityReadonly(child).Handle.Id);
        Assert.Equal(0, world.GetEntityReadonly(grandChild).Handle.Id);

        Assert.False(world.IsValid(parent));
        Assert.False(world.IsValid(child));
        Assert.False(world.IsValid(grandChild));
    }

    [Fact]
    public void ActiveSelf_propagates_to_children()
    {
        var parent = world.CreateEntityByHandle();
        ref Entity entity = ref world.GetEntity(parent);
        var child = world.CreateEntityByHandle(parent);
        var grandChild = world.CreateEntityByHandle(child);

        entity.ActiveSelf = true;

        Assert.True(world.GetEntity(parent).ActiveInHierachy);
        Assert.True(world.GetEntity(child).ActiveInHierachy);
        Assert.True(world.GetEntity(grandChild).ActiveInHierachy);

        world.GetEntity(parent).ActiveSelf = false;

        Assert.False(world.GetEntity(parent).ActiveInHierachy);
        Assert.False(world.GetEntity(child).ActiveInHierachy);
        Assert.False(world.GetEntity(grandChild).ActiveInHierachy);
    }

    [Fact]
    public void Child_can_be_active_self_but_inactive_in_hierarchy()
    {
        var parent = world.CreateEntityByHandle();
        var child = world.CreateEntityByHandle();

        world.GetEntity(child).TrySetParentFirst(parent);

        world.GetEntity(parent).ActiveSelf = false;

        Assert.False(world.GetEntity(parent).ActiveSelf);
        Assert.False(world.GetEntity(parent).ActiveInHierachy);

        Assert.True(world.GetEntity(child).ActiveSelf);
        Assert.False(world.GetEntity(child).ActiveInHierachy);

        world.GetEntity(parent).ActiveSelf = true;

        Assert.True(world.GetEntity(parent).ActiveInHierachy);
        Assert.True(world.GetEntity(child).ActiveInHierachy);
    }

    [Fact]
    public void Activating_child_does_not_override_inactive_parent()
    {
        var parent = world.CreateEntityByHandle();
        var child = world.CreateEntityByHandle();

        world.GetEntity(child).TrySetParentFirst(parent);

        world.GetEntity(parent).ActiveSelf = false;
        world.GetEntity(child).ActiveSelf = false;

        world.GetEntity(child).ActiveSelf = true;

        Assert.True(world.GetEntity(child).ActiveSelf);
        Assert.False(world.GetEntity(child).ActiveInHierachy);
    }

    [Fact]
    public void Children_are_iterated_in_order()
    {
        /*
        parent
        ├── child a
        │   ├── child b
        │   └── child c
        │       ├── child d
        │       │   └── child e
        │       └── child f
        ├── child g
        │   └── child h
        └── child i
        */

        var parent = world.CreateEntityByHandle(new StringID("Parent"));

        var childA = world.CreateEntityByHandle(parent, new StringID("Child a"));
        var childB = world.CreateEntityByHandle(childA, new StringID("Child b"));
        var childC = world.CreateEntityByHandle(childA, new StringID("Child c"));

        var childD = world.CreateEntityByHandle(childC, new StringID("Child d"));
        var childE = world.CreateEntityByHandle(childD, new StringID("Child e"));
        var childF = world.CreateEntityByHandle(childC, new StringID("Child f"));
        var childG = world.CreateEntityByHandle(parent, new StringID("Child g"));
        var childH = world.CreateEntityByHandle(childG, new StringID("Child h"));
        var childI = world.CreateEntityByHandle(parent, new StringID("Child i"));

        ref var parentEntity = ref world.GetEntity(parent);

        var result = new List<Handle<Entity>>();

        foreach (var child in parentEntity.Children)
            result.Add(child.Handle);

        Assert.Equal([childA, childG, childI], result);
    }

    [Fact]
    public void Children_are_iterated_recursively_in_order()
    {
        /*
        parent
        ├── child a
        │   ├── child b
        │   └── child c
        │       ├── child d
        │       │   └── child e
        │       └── child f
        ├── child g
        │   └── child h
        └── child i
        */

        var parent = world.CreateEntityByHandle(new StringID("Parent"));

        var childA = world.CreateEntityByHandle(parent, new StringID("Child a"));
        var childB = world.CreateEntityByHandle(childA, new StringID("Child b"));
        var childC = world.CreateEntityByHandle(childA, new StringID("Child c"));

        var childD = world.CreateEntityByHandle(childC, new StringID("Child d"));
        var childE = world.CreateEntityByHandle(childD, new StringID("Child e"));
        var childF = world.CreateEntityByHandle(childC, new StringID("Child f"));
        var childG = world.CreateEntityByHandle(parent, new StringID("Child g"));
        var childH = world.CreateEntityByHandle(childG, new StringID("Child h"));
        var childI = world.CreateEntityByHandle(parent, new StringID("Child i"));

        var result = new List<Handle<Entity>>();

        void Visit(Handle<Entity> handle)
        {
            foreach (var child in world.GetEntity(handle).Children)
            {
                result.Add(child.Handle);
                Visit(child.Handle);
            }
        }

        Visit(parent);

        Assert.Equal(
            [
            childA,
            childB,
            childC,
            childD,
            childE,
            childF,
            childG,
            childH,
            childI
            ],
            result);
    }
}
