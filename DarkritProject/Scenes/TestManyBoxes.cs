using System;
using System.IO;
using Darkrit.Base;
using Darkrit.DevTools.Logger;
using Darkrit.Graphics;
using Darkrit.InputSystem;
using Darkrit.InputSystem.Bindings;
using Darkrit.Math;
using Darkrit.Physics.Boxy2D;
using Darkrit.Utilities;
using FontStashSharp;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GamepadButton = Microsoft.Xna.Framework.Input.Buttons;
using Key = Microsoft.Xna.Framework.Input.Keys;
using Sprite = Darkrit.Graphics.Sprite;

namespace Darkrit.Scenes;

internal class TestManyBoxes : Scene
{
    static bool paused = false;
    static bool render = true;

    enum ItemType
    {
        Player,
        Wall1,
        Wall2
    };

    AnimatedSprite slimeAnimation;
    Vector2 position = new Vector2(-200, 0);
    Vector2 previousPosition;
    Vector2 velocity;
    private float speed = 500f;
    InputAction moveUp;
    InputAction moveDown;
    InputAction moveLeft;
    InputAction moveRight;
    FontSystem _fontSystem;
    Camera camera = new();
    Handle<Body<ItemType>> playerHandle;
    Darkrit.Physics.Boxy2D.World<ItemType> world;

    public override void Initialize()
    {
        //position = new Vector2(Core.GraphicsDevice.Viewport.Width * 0.5f, Core.GraphicsDevice.Viewport.Height * 0.5f);

        // Create the texture atlas from the XML configuration file.
        TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");

        _fontSystem = new FontSystem();
        _fontSystem.AddFont(File.ReadAllBytes(Path.Combine(Content.RootDirectory, @"fonts/FiraCode-Regular.ttf")));

        // Create the animated sprite for the slime from the atlas.
        
        slimeAnimation = atlas.CreateAnimatedSprite("slime-animation");
        slimeAnimation.Scale = new Vector2(4.0f, 4.0f);

        world = new();
        world.Create(new Vector2(0, 0), new Vector2(600, 20), (uint)CollisionLayer.World, userData: ItemType.Wall1);
        world.Create(new Vector2(0, 0), new Vector2(30, 200), (uint)CollisionLayer.World, userData: ItemType.Wall2);

        playerHandle = world.Create(position, slimeAnimation.Size, (uint)CollisionLayer.None, (uint)CollisionLayer.World, userData: ItemType.Player);

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

        float value = Input.GetAxis(moveLeft, moveRight);
        Vector2 ve2Value = Input.GetVector(moveLeft, moveRight, moveDown, moveUp);
    }

    public override void FixedUpdate(GameTime gameTime)
    {
        base.FixedUpdate(gameTime);

        if (paused) return;

        slimeAnimation.Update(gameTime);
        HandleInput(gameTime);
    }

    private void HandleInput(GameTime gameTime)
    {
        previousPosition = position;

        if (moveUp.IsPressed)
        {
            velocity.Y = -1;
        }
        else if (moveDown.IsPressed)
        {
            velocity.Y = 1;
        }
        else
            velocity.Y = 0;

        if (moveLeft.IsPressed)
        {
            velocity.X = -1;
        }
        else if (moveRight.IsPressed)
        {
            velocity.X = 1;
        }
        else
            velocity.X = 0;

        position += velocity.Normalized * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

        //position = camera.ScreenToWorld(Core.Input.GetMousePosition(), Core.Viewport) - new Vector2(slimeAnimation.Width * 0.5f, slimeAnimation.Height * 0.5f);

        //ref var r = ref world.Get(playerHandle);
        //r.Location = position;
        var hasCollision = world.Move(playerHandle, position, Test.Slide);
        position = world.Get(playerHandle).Bounds.Location;
        if (hasCollision)
        {
            var latestCollisions = world.LastCollsions;
            foreach (var item in latestCollisions)
            {
                Log.Info($"Collision with {world.Get(item.Handle).UserData}");
            }
        }
    }

    bool useInterpolation = true;

    public override void EditorDraw(GameTime gameTime)
    {
        ImGui.Begin("Test");
        ImGui.Checkbox("Render", ref render);
        ImGui.Checkbox("Pause", ref paused);
        ImGui.Checkbox("Enable Physics Interpolation", ref useInterpolation);
        ImGui.End();

        camera.EditorDraw();
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        if (!render) return;

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.GetViewMatrix(Core.Viewport), rasterizerState: RasterizerState.CullNone);
        
        if(!useInterpolation)
        {
            slimeAnimation.Draw(Core.SpriteBatch, position);
            Core.SpriteBatch.DrawString(_fontSystem.GetFont(17.5f), $"Position: {position}", position + new Vector2(0, -30), Color.White);
        }
        else
        {
            Vector2 lerpedPosition = Vector2.Lerp(previousPosition, position, Core.FixedUpdateAlpha);
            slimeAnimation.Draw(Core.SpriteBatch, lerpedPosition);
            Core.SpriteBatch.DrawString(_fontSystem.GetFont(17.5f), $"Position: {position}", lerpedPosition + new Vector2(0, -30), Color.White);
        }

        world.Draw(Core.SpriteBatch);
        Core.SpriteBatch.End();
    }

    public override void Deinitialize()
    {
    }
}
