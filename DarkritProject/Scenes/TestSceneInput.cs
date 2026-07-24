using Darkrit.Graphics;
using Darkrit.InputSystem;
using Darkrit.InputSystem.Bindings;
using Darkrit.Utilities;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GamepadButton = Microsoft.Xna.Framework.Input.Buttons;
using Key = Microsoft.Xna.Framework.Input.Keys;

namespace Darkrit.Scenes
{
    internal class TestSceneInput : Scene
    {
        static bool paused = false;
        static bool render = true;

        AnimatedSprite slimeAnimation;
        Vector2 position;
        Vector2 velocity;
        private float speed = 500f;
        InputAction moveUp;
        InputAction moveDown;
        InputAction moveLeft;
        InputAction moveRight;

        public override void Initialize()
        {
            position = new Vector2(Core.GraphicsDevice.Viewport.Width * 0.5f, Core.GraphicsDevice.Viewport.Height * 0.5f);

            // Create the texture atlas from the XML configuration file.
            TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");


            // Create the animated sprite for the slime from the atlas.
            slimeAnimation = atlas.CreateAnimatedSprite("slime-animation");
            slimeAnimation.Scale = new Vector2(4.0f, 4.0f);

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

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (paused) return;

            slimeAnimation.Update(gameTime);
            HandleInput(gameTime);
        }

        private void HandleInput(GameTime gameTime)
        {
            if (moveUp.IsPressed)
                velocity.Y = -1;
            else if (moveDown.IsPressed)
                velocity.Y = 1;
            else
                velocity.Y = 0;

            if (moveLeft.IsPressed)
                velocity.X = -1;
            else if (moveRight.IsPressed)
                velocity.X = 1;
            else
                velocity.X = 0;

            position += velocity.Normalized * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void DebugDraw(GameTime gameTime)
        {
            ImGui.Begin("Test");
            ImGui.Checkbox("Render", ref render);
            ImGui.Checkbox("Pause", ref paused);

            ImGui.End();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

            if (!render) return;

            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            slimeAnimation.Draw(Core.SpriteBatch, position);
            Core.SpriteBatch.End();
        }

        public override void Deinitialize()
        {
        }
    }
}
