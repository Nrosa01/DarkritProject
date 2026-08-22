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
        ref var ref1 = ref Entity.AddComponent<ComponentWithValueData>();
        ref var ref2 = ref Entity.AddComponent<ComponentWithValueData>();

        ref1.firstData = 4;
        ref2.firstData = 7;

        Assert.Equal(4, ref1.firstData);
        Assert.Equal(7, ref2.firstData);
    }

    [Fact]
    public void Parameterless_get_component_removes_first_ocurrence()
    {
        ref var component1 = ref Entity.AddComponent<ComponentA>();
        ref var component2 = ref Entity.AddComponent<ComponentA>();

        Entity.RemoveComponent<ComponentA>();
        Assert.True(Entity.HasComponent<ComponentA>());
        Assert.False(Entity.HasComponent<ComponentA>(component1.Handle));
        Assert.True(Entity.HasComponent<ComponentA>(component2.Handle));
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
    public void Entity_doesnt_allow_recursive_parent()
    {
        ref Entity parent = ref world.CreateEntity();
        ref Entity child = ref world.CreateEntity();

        parent.TryAddChild(child.Handle);
        Assert.False(child.TryAddChild(parent.Handle));
    }

    [Fact]
    public void Entity_doesnt_allow_recursive_parent_through_multiple_levels()
    {
        ref Entity root = ref world.CreateEntity();
        ref Entity parent = ref world.CreateEntity();
        ref Entity child = ref world.CreateEntity();
        ref Entity leaf = ref world.CreateEntity();

        root.TryAddChild(parent.Handle);
        parent.TryAddChild(child.Handle);
        child.TryAddChild(leaf.Handle);

        Assert.True(leaf.TrySetParent(root.Handle));
    }

    [Fact]
    public void Entity_doesnt_allow_to_add_parent_as_child()
    {
        ref Entity root = ref world.CreateEntity();
        ref Entity parent = ref world.CreateEntity();
        ref Entity child = ref world.CreateEntity();
        ref Entity leaf = ref world.CreateEntity();

        root.TryAddChild(parent.Handle);
        parent.TryAddChild(child.Handle);
        child.TryAddChild(leaf.Handle);

        Assert.False(leaf.TryAddChild(root.Handle));
    }

    [Fact]
    public void Entity_doesnt_allow_reparenting_to_descendant()
    {
        ref Entity root = ref world.CreateEntity();
        ref Entity parent = ref world.CreateEntity();
        ref Entity child = ref world.CreateEntity();
        ref Entity leaf = ref world.CreateEntity();

        root.TryAddChild(parent.Handle);
        parent.TryAddChild(child.Handle);
        child.TryAddChild(leaf.Handle);

        Assert.False(root.TrySetParent(leaf.Handle));
    }

    [Fact]
    public void Destroy_entity_also_destroys_children()
    {
        var parent = world.CreateEntityByHandle();
        var child = world.CreateEntityByHandle();
        var grandChild = world.CreateEntityByHandle();

        world.GetEntity(child).TrySetParentFirst(parent);
        world.GetEntity(grandChild).TrySetParentFirst(child);

        Assert.True(world.TryRemoveEntity(parent));

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

        Assert.True(world.GetEntity(parent).ActiveInHierarchy);
        Assert.True(world.GetEntity(child).ActiveInHierarchy);
        Assert.True(world.GetEntity(grandChild).ActiveInHierarchy);

        world.GetEntity(parent).ActiveSelf = false;

        Assert.False(world.GetEntity(parent).ActiveInHierarchy);
        Assert.False(world.GetEntity(child).ActiveInHierarchy);
        Assert.False(world.GetEntity(grandChild).ActiveInHierarchy);
    }

    [Fact]
    public void Child_can_be_active_self_but_inactive_in_hierarchy()
    {
        var parent = world.CreateEntityByHandle();
        var child = world.CreateEntityByHandle();

        world.GetEntity(child).TrySetParentFirst(parent);

        world.GetEntity(parent).ActiveSelf = false;

        Assert.False(world.GetEntity(parent).ActiveSelf);
        Assert.False(world.GetEntity(parent).ActiveInHierarchy);

        Assert.True(world.GetEntity(child).ActiveSelf);
        Assert.False(world.GetEntity(child).ActiveInHierarchy);

        world.GetEntity(parent).ActiveSelf = true;

        Assert.True(world.GetEntity(parent).ActiveInHierarchy);
        Assert.True(world.GetEntity(child).ActiveInHierarchy);
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
        Assert.False(world.GetEntity(child).ActiveInHierarchy);
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

        var parent = world.CreateEntityByHandle("Parent");

        var childA = world.CreateEntityByHandle(parent, "Child a");
        var childB = world.CreateEntityByHandle(childA, "Child b");
        var childC = world.CreateEntityByHandle(childA, "Child c");

        var childD = world.CreateEntityByHandle(childC, "Child d");
        var childE = world.CreateEntityByHandle(childD, "Child e");
        var childF = world.CreateEntityByHandle(childC, "Child f");
        var childG = world.CreateEntityByHandle(parent, "Child g");
        var childH = world.CreateEntityByHandle(childG, "Child h");
        var childI = world.CreateEntityByHandle(parent, "Child i");

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

        var parent = world.CreateEntityByHandle("Parent");

        var childA = world.CreateEntityByHandle(parent, "Child a");
        var childB = world.CreateEntityByHandle(childA, "Child b");
        var childC = world.CreateEntityByHandle(childA, "Child c");

        var childD = world.CreateEntityByHandle(childC, "Child d");
        var childE = world.CreateEntityByHandle(childD, "Child e");
        var childF = world.CreateEntityByHandle(childC, "Child f");
        var childG = world.CreateEntityByHandle(parent, "Child g");
        var childH = world.CreateEntityByHandle(childG, "Child h");
        var childI = world.CreateEntityByHandle(parent, "Child i");

        var result = new List<Handle<Entity>>();

        world.TraverseHierarchy(parent, result.Add);

        Assert.Equal(
            [
            parent,
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

    [Fact]
    public void Hierarchy_based_update_propagates_parent_changes()
    {
        world.UseHierarchyScheduler = true;

        ref var a = ref world.CreateEntity();
        a.AddComponent<ComponentWithValueData>();
        ref var b = ref world.CreateEntity(a.Handle);
        b.AddComponent<ComponentWithValueData>();
        ref var c = ref world.CreateEntity(a.Handle);
        c.AddComponent<ComponentWithValueData>();

        world.Update(default);
        
        Assert.Equal(2, a.GetComponent<ComponentWithValueData>().firstData);
        Assert.Equal(3, b.GetComponent<ComponentWithValueData>().firstData);
        Assert.Equal(3, c.GetComponent<ComponentWithValueData>().firstData);

        c.TrySetParent(b.Handle);

        world.Update(default);

        Assert.Equal(3, a.GetComponent<ComponentWithValueData>().firstData);
        Assert.Equal(4, b.GetComponent<ComponentWithValueData>().firstData);
        Assert.Equal(5, c.GetComponent<ComponentWithValueData>().firstData);
    }

    [Fact]
    public void Component_OnEnable_and_OnDisable_are_called()
    {
        ref var entity = ref world.CreateEntity("A");
        ref var entityb = ref world.CreateEntity(entity.Handle, "B");
        entityb.AddComponent<ActivatableComponent>();
        ref var component = ref entityb.GetComponent<ActivatableComponent>();
        Assert.Equal(0, component.enabledTimes);
        Assert.Equal(0, component.disabledTimes);
        
        // Make sure that it doesn't call when it was already enabled
        component.Enabled = true;
        Assert.Equal(0, component.enabledTimes);
        
        // When parents are active, disable should trigger the OnDisable callback
        component.Enabled = false;
        Assert.Equal(0, component.enabledTimes);
        Assert.Equal(1, component.disabledTimes);

        // When parents are active and script was disabled, enable should trigger the OnEnable callback
        component.Enabled = true;
        Assert.Equal(1, component.enabledTimes);
        Assert.Equal(1, component.disabledTimes);

        // Component OnDisable should be called when a parent entity is disabled
        entity.ActiveSelf = false;
        Assert.Equal(1, component.enabledTimes);
        Assert.Equal(2, component.disabledTimes);

        // Nothing should happen here
        entityb.ActiveSelf = false;
        Assert.Equal(1, component.enabledTimes);
        Assert.Equal(2, component.disabledTimes); // Falla aquí

        // Nothing should happen here because b is disabled
        entity.ActiveSelf = true;
        Assert.Equal(1, component.enabledTimes);
        Assert.Equal(2, component.disabledTimes);

        // Nothing should happen hnere because b is disabled
        component.Enabled = true;
        component.Enabled = false;
        component.Enabled = true;
        Assert.Equal(1, component.enabledTimes);
        Assert.Equal(2, component.disabledTimes);

        // Given parents are enabled and component is enabled, OnEnable should trigger now
        entityb.ActiveSelf = true;
        Assert.Equal(2, component.enabledTimes);
        Assert.Equal(2, component.disabledTimes);
    }
}
