using System.Numerics;
using Darkrit.Physics.Boxy2D;

using RectangleF = Darkrit.Math.RectangleF;

namespace DakritTests.Physics.Boxy2D;

public class CollisionFunctionsTest
{
    [Fact]
    public void SweeptAABB_WhenMovingRightIntoBody_ReturnsCollision()
    {
        var body = new RectangleF
        {
            X = 0,
            Y = 0,
            Size = new Vector2(10, 10)
        };

        var obstacle = new RectangleF
        {
            X = 20,
            Y = 0,
            Size = new Vector2(10, 10)
        };

        var delta = new Vector2(20, 0);

        var response = CollisionFunctions.SweeptAABB(
            body,
            obstacle,
            delta);

        Assert.True(response.HasCollision);
        Assert.Equal(0.5f, response.CollisionTime);
        Assert.Equal(new Vector2(-1, 0), response.Normal);
    }

    [Fact]
    public void SweeptAABB_WhenMovingLeftIntoBody_ReturnsCollision()
    {
        var body = new RectangleF
        {
            X = 20,
            Y = 0,
            Size = new Vector2(10, 10)
        };

        var obstacle = new RectangleF
        {
            X = 0,
            Y = 0,
            Size = new Vector2(10, 10)
        };

        var delta = new Vector2(-20, 0);

        var response = CollisionFunctions.SweeptAABB(
            body,
            obstacle,
            delta);

        Assert.True(response.HasCollision);
        Assert.Equal(0.5f, response.CollisionTime);
        Assert.Equal(new Vector2(1, 0), response.Normal);
    }

    [Fact]
    public void SweeptAABB_WhenMovingDownIntoBody_ReturnsCollision()
    {
        var body = new RectangleF
        {
            X = 0,
            Y = 0,
            Size = new Vector2(10, 10)
        };

        var obstacle = new RectangleF
        {
            X = 0,
            Y = 20,
            Size = new Vector2(10, 10)
        };

        var delta = new Vector2(0, 20);

        var response = CollisionFunctions.SweeptAABB(
            body,
            obstacle,
            delta);

        Assert.True(response.HasCollision);
        Assert.Equal(0.5f, response.CollisionTime);
        Assert.Equal(new Vector2(0, -1), response.Normal);
    }

    [Fact]
    public void SweeptAABB_WhenMovingUpIntoBody_ReturnsCollision()
    {
        var body = new RectangleF
        {
            X = 0,
            Y = 20,
            Size = new Vector2(10, 10)
        };

        var obstacle = new RectangleF
        {
            X = 0,
            Y = 0,
            Size = new Vector2(10, 10)
        };

        var delta = new Vector2(0, -20);

        var response = CollisionFunctions.SweeptAABB(
            body,
            obstacle,
            delta);

        Assert.True(response.HasCollision);
        Assert.Equal(0.5f, response.CollisionTime);
        Assert.Equal(new Vector2(0, 1), response.Normal);
    }

    [Fact]
    public void SweeptAABB_WhenMovingHorizontallyAndSeparatedOnY_ReturnsNoCollision()
    {
        var body = new RectangleF
        {
            X = 0,
            Y = 0,
            Size = new Vector2(10, 10)
        };

        var obstacle = new RectangleF
        {
            X = 20,
            Y = 20,
            Size = new Vector2(10, 10)
        };

        var delta = new Vector2(30, 0);

        var response = CollisionFunctions.SweeptAABB(
            body,
            obstacle,
            delta);

        Assert.False(response.HasCollision);
        Assert.Equal(-1.0f, response.CollisionTime);
    }

    [Fact]
    public void SweeptAABB_WhenMovingVerticallyAndSeparatedOnX_ReturnsNoCollision()
    {
        var body = new RectangleF
        {
            X = 0,
            Y = 0,
            Size = new Vector2(10, 10)
        };

        var obstacle = new RectangleF
        {
            X = 20,
            Y = 20,
            Size = new Vector2(10, 10)
        };

        var delta = new Vector2(0, 30);

        var response = CollisionFunctions.SweeptAABB(
            body,
            obstacle,
            delta);

        Assert.False(response.HasCollision);
        Assert.Equal(-1.0f, response.CollisionTime);
    }

    [Fact]
    public void SweeptAABB_WhenMovingWithoutReachingBody_ReturnsNoCollision()
    {
        var body = new RectangleF
        {
            X = 0,
            Y = 0,
            Size = new Vector2(10, 10)
        };

        var obstacle = new RectangleF
        {
            X = 30,
            Y = 0,
            Size = new Vector2(10, 10)
        };

        var delta = new Vector2(10, 0);

        var response = CollisionFunctions.SweeptAABB(
            body,
            obstacle,
            delta);

        Assert.False(response.HasCollision);
        Assert.Equal(-1.0f, response.CollisionTime);
    }

    [Fact]
    public void SweeptAABB_WhenMovingDiagonallyIntoCorner_ReturnsCollision()
    {
        var body = new RectangleF
        {
            X = 0,
            Y = 0,
            Size = new Vector2(10, 10)
        };

        var obstacle = new RectangleF
        {
            X = 20,
            Y = 20,
            Size = new Vector2(10, 10)
        };

        var delta = new Vector2(20, 20);

        var response = CollisionFunctions.SweeptAABB(
            body,
            obstacle,
            delta);

        Assert.True(response.HasCollision);
        Assert.Equal(0.5f, response.CollisionTime);
    }
}