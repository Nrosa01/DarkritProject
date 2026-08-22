using Darkrit.EntityModel;
using Microsoft.Xna.Framework;

namespace Dakrit.Tests.EntityModel;

[Component]
partial struct ComponentA
{

}

[Component]
[Priority(-1)]
partial struct ComponentB
{
    public int test = 1;

    public void Update(GameTime gameTime)
    {
        test++;
    }

    public ComponentB()
    {
    }
}

[Component]
[Priority(1)]
partial struct ComponentC
{
    public int test = 1;

    public void Update(GameTime gameTime)
    {
        test++;
    }

    public ComponentC()
    {
    }
}

[Component]
partial struct ActivatableComponent
{
    public int enabledTimes = 0;
    public int disabledTimes = 0;

    public void OnEnable()
    {
        enabledTimes++;
    }

    public void OnDisable()
    {
        disabledTimes++;
    }
}

[Component]
[InjectComponent(typeof(ComponentB))]
[InjectComponent(typeof(ComponentC))]
partial struct ComponentD
{
    public int test = 1;
    public int test2 = 1;

    public void Update(GameTime gameTime)
    {
        test += ComponentB.test;
        test2 += ComponentC.test;
    }

    public ComponentD()
    {
    }
}

[Component]
partial struct UpdateableComponent
{
    public void Update(GameTime gameTime)
    {
        throw new Exception();
    }
}

[Component]
partial struct FixedUpdateableComponent
{
    public void FixedUpdate(GameTime gameTime)
    {
        throw new Exception();
    }
}


[Component]
partial struct DrawableComponent
{
    public void Draw(GameTime gameTime)
    {
        throw new Exception();
    }
}

[Component]
partial struct ComponentWithValueData
{
    public int firstData = 1;
    public int secondData = 2;

    public void Update(GameTime gameTime)
    {
        if(Entity.HasParent)
        {
            firstData = Entity.Parent.GetComponent<ComponentWithValueData>().firstData + 1;
        }
        else
        {
            firstData++;
        }
    }

    public ComponentWithValueData()
    {
    }
}

[Component]
partial struct ComponentWithReferenceData
{
    public List<int> list = [2, 4, 6];

    public ComponentWithReferenceData()
    {
    }
}