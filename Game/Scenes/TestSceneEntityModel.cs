using System.Diagnostics;
using Darkrit;
using Darkrit.Base;
using Darkrit.DevTools.Logger;
using Darkrit.EntityModel;
using Darkrit.Graphics;
using Darkrit.InputSystem;
using Darkrit.InputSystem.Bindings;
using Darkrit.Scenes;
using Darkrit.Utilities;
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

    public void Draw(GameTime gameTime)
    {
        Sprite.Draw(Core.SpriteBatch, Entity.Position);
    }
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

public class TestSceneEntityModel : Scene
{
    EntityRegistry entityWorld;
    Handle<Entity> player;
    Camera camera = new();

    public override void Initialize()
    {
        TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");

        entityWorld = new(10);
        player = entityWorld.CreateEntity();
        ref Entity playerRef = ref entityWorld.GetEntity(player);
        var h1 = playerRef.AddComponent<SpriteComponent>(new SpriteComponent
        {
            Sprite = atlas.CreateAnimatedSprite("slime-animation")
        });
        var h2 =playerRef.AddComponent<PlayerController>(new PlayerController());
        Debug.Assert(h1.Id != 0);
        Debug.Assert(h2.Id != 0);
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
