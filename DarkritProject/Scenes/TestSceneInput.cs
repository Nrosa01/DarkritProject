using Darkrit.Input;
using Darkrit.Input.Bindings;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.Utilities;
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

            moveUp = new([new KeyboardBinding(Key.Up, Core.Input), new KeyboardBinding(Key.W, Core.Input)]);
            moveDown = new([new KeyboardBinding(Key.Down, Core.Input), new KeyboardBinding(Key.S, Core.Input)]);
            moveLeft = new([new KeyboardBinding(Key.Left, Core.Input), new KeyboardBinding(Key.A, Core.Input)]);
            moveRight = new([new KeyboardBinding(Key.Right, Core.Input), new KeyboardBinding(Key.D, Core.Input)]);
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
            if (moveUp.Pressed())
                velocity.Y = -1;
            else if (moveDown.Pressed())
                velocity.Y = 1;
            else
                velocity.Y = 0;

            if (moveLeft.Pressed())
                velocity.X = -1;
            else if (moveRight.Pressed())
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
