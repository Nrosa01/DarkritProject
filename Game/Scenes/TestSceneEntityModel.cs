using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Darkrit;
using Darkrit.Base;
using Darkrit.DevTools.Logger;
using Darkrit.EntityModel;
using Darkrit.Graphics;
using Darkrit.InputSystem;
using Darkrit.InputSystem.Bindings;
using Darkrit.Physics.Boxy2D;
using Darkrit.Scenes;
using Darkrit.Utilities;
using DarkritGame.Scenes;
using Gum.DataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GamepadButton = Microsoft.Xna.Framework.Input.Buttons;
using Key = Microsoft.Xna.Framework.Input.Keys;

namespace DarkritGame.Scenes;

[Component]
public partial struct SpriteComponent
{
    public AnimatedSprite Sprite;

    public void Start()
    {
        Sprite.Scale = Vector2.One * 4;
    }

    public void FixedUpdate(GameTime gameTime) => Sprite.Update(gameTime);

    public void Draw(GameTime gameTime) => Sprite.Draw(Core.SpriteBatch, Entity.Position);
}

[Component]
public partial struct PlayerController
{
    InputAction moveUp;
    InputAction moveDown;
    InputAction moveLeft;
    InputAction moveRight;

    Vector2 direction;
    private readonly float speed = 500f;

    public PlayerController()
    {
    }

    public void Start()
    {
        moveUp = Core.Input.CreateAction("Move Up").AddBindings([
            new KeyboardBinding(Key.Up),
            new KeyboardBinding(Key.W),
            new GamepadBinding(GamepadButton.DPadUp),
            new GamepadBinding(GamepadButton.LeftThumbstickUp),
        ]);

        moveDown = Core.Input.CreateAction("Move Down").AddBindings([
            new KeyboardBinding(Key.Down),
            new KeyboardBinding(Key.S),
            new GamepadBinding(GamepadButton.DPadDown),
            new GamepadBinding(GamepadButton.LeftThumbstickDown),
        ]);

        moveLeft = Core.Input.CreateAction("Move Left").AddBindings([
            new KeyboardBinding(Key.Left),
            new KeyboardBinding(Key.A),
            new GamepadBinding(GamepadButton.DPadLeft),
            new GamepadBinding(GamepadButton.LeftThumbstickLeft),
        ]);

        moveRight = Core.Input.CreateAction("Move Right").AddBindings([
            new KeyboardBinding(Key.Right),
            new KeyboardBinding(Key.D),
            new GamepadBinding(GamepadButton.DPadRight),
            new GamepadBinding(GamepadButton.LeftThumbstickRight),
        ]);
    }

    public void FixedUpdate(GameTime gameTime)
    {
        if (moveUp.IsPressed)
        {
            direction.Y = -1;
        }
        else if (moveDown.IsPressed)
        {
            direction.Y = 1;
        }
        else
            direction.Y = 0;

        if (moveLeft.IsPressed)
        {
            direction.X = -1;
        }
        else if (moveRight.IsPressed)
        {
            direction.X = 1;
        }
        else
            direction.X = 0;

        Entity.Position += direction.Normalized * speed * gameTime.Delta;
    }
}


[Component]
[InjectComponent(typeof(SquareRenderer))]
public partial struct Mover
{
    static readonly int WindowsWidth = Core.GraphicsDevice.Viewport.Width;
    static readonly int WindowsHeight = Core.GraphicsDevice.Viewport.Height;

    Handle<SquareRenderer> squareHandle;
    ComponentStore<SquareRenderer> store;

    public Vector2 Velocity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(GameTime gameTime)
    {
        var size = SquareRenderer.Size;

        Entity.Transform.Position.X += Velocity.X;
        Entity.Transform.Position.Y += Velocity.Y;

        if (Entity.Transform.Position.X < 0 || Entity.Transform.Position.X + size > WindowsWidth)
            Velocity.X *= -1;

        if (Entity.Transform.Position.Y < 0 || Entity.Transform.Position.Y + size > WindowsHeight)
            Velocity.Y *= -1;
    }
}

[Component]
public partial struct SquareRenderer
{
    public int Size;
    public readonly void Draw(GameTime gameTime)
    {
        var pos = Entity.Transform.Position;
        float r = pos.X - MathF.Floor(pos.X);
        float g = pos.Y - MathF.Floor(pos.Y);

        var color = new Color(r, g, pos.X);
        Core.SpriteBatch.Draw(Core.Pixel, new Rectangle((int)pos.X, (int)pos.Y, Size, Size), null, color);
    }
}


public class TestSceneEntityModel : Scene
{
    EntityRegistry entityWorld;
    Handle<Entity> player;
    Camera camera = new();

    public override void Initialize()
    {
        TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");

        entityWorld = new(10);
        //player = entityWorld.CreateEntity();
        //ref Entity playerRef = ref entityWorld.GetEntity(player);
        //playerRef.AddComponent<SpriteComponent>(new SpriteComponent
        //{
        //    Sprite = atlas.CreateAnimatedSprite("slime-animation")
        //});
        //playerRef.AddComponent<PlayerController>(new PlayerController());


        for (var i = 0; i < 10_000; i++)
        {
            var entity = entityWorld.CreateEntityByHandle();
            ref Entity instanceEntity = ref entityWorld.GetEntity(entity);

            instanceEntity.AddComponent(new Mover { Velocity = new Vector2(8 + i * 0.01f, 4f + i * 0.01f) });
            instanceEntity.AddComponent(new SquareRenderer { Size = 10 });
        }
    }

    public override void Update(GameTime gameTime) => entityWorld.Update(gameTime);

    public override void FixedUpdate(GameTime gameTime) => entityWorld.FixedUpdate(gameTime);

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.GetViewMatrix(Core.Viewport), rasterizerState: RasterizerState.CullNone);
        entityWorld.Draw(gameTime);
        Core.SpriteBatch.End();

        base.Draw(gameTime);
    }

    public override void EditorDraw(GameTime gameTime)
    {
        base.EditorDraw(gameTime);
        camera.EditorDraw();
    }

    public override void Deinitialize() { }
}
