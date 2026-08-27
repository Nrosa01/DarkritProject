using System;
using Darkrit.EntityModel;
using Darkrit.EntityModel.Components;
using Microsoft.Xna.Framework;

namespace Game.Components;

[Component]
[InjectComponent(typeof(PhysicsBody))]
public partial struct MovingPlatform
{
    [SerializeField] float amplitude = 100f;
    [SerializeField] float speed = 1f;

    Vector2 startPosition;

    /// <inheritdoc/>
    public void OnAdd()
    {
        startPosition = Entity.Position;
    }

    public void FixedUpdate(GameTime gameTime)
    {
        var timer = (float)gameTime.TotalGameTime.TotalSeconds;

        Entity.Position = startPosition with
        {
            X = startPosition.X + MathF.Sin(timer * speed) * amplitude
        };
    }
}