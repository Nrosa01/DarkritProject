using Darkrit.EntityModel;
using Microsoft.Xna.Framework;

namespace Dakrit.Tests.EntityModel;

[Component]
partial struct ComponentA
{

}

[Component]
[InjectComponent(typeof(ComponentA))]
partial struct ComponentBThatRequiresA
{

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