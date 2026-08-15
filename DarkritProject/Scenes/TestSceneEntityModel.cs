using Darkrit;
using Darkrit.Base;
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

public static class ComponentExtensions
{
    extension(IComponent component)
    {
        public ref Entity Entity => ref component.World.GetEntity(component.EntityHandle);
    }
}

[Renderable, Updateable]
public struct SpriteComponent : IComponent
{
    public EntityRegistry World { get; set; }
    public Handle<Entity> EntityHandle { get; set; }

    public bool Enabled { get; set; }

    public AnimatedSprite Sprite;

    public static bool Renderable { get; set; } = true;

    public void Start()
    {
        Sprite.Scale = Vector2.One * 4;
    }

    public void FixedUpdate(GameTime gameTime)
    {
        Sprite.Update(gameTime);
    }

    public void Update(GameTime gameTime)
    {
    }

    public void Draw(GameTime gameTime)
    {
        Sprite.Draw(Core.SpriteBatch, this.Entity.Position);
    }
}

[Updateable]
public struct PlayerController : IComponent
{
    public EntityRegistry World { get; set; }
    public Handle<Entity> EntityHandle { get; set; }
    public bool Enabled { get; set; }

    public static bool Updateable { get; set; } = true;


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

        this.Entity.Position += direction.Normalized * speed * gameTime.Delta;
    }

    public void Update(GameTime gameTime)
    {
    }

    public void Draw(GameTime gameTime)
    {

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
        playerRef.AddComponent<SpriteComponent>(new SpriteComponent
        {
            Sprite = atlas.CreateAnimatedSprite("slime-animation")
        });
        playerRef.AddComponent<PlayerController>(new PlayerController());
    }

    public override void Deinitialize()
    {
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        entityWorld.Update(gameTime);
    }

    public override void FixedUpdate(GameTime gameTime)
    {
        base.FixedUpdate(gameTime);
        entityWorld.FixedUpdate(gameTime);
    }

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
}
