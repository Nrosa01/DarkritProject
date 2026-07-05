using System;
using System.Collections.Generic;
using System.Text;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.Utilities;

namespace DungeonSlime.Scenes
{
    internal class TestScene : Scene
    {
        AnimatedSprite slimeAnimation;
        Vector2 position;
        Vector2 velocity;
        private float speed = 500f;

        public override void LoadContent()
        {
            base.LoadContent();
            // Create the texture atlas from the XML configuration file.
            TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");

            // Create the animated sprite for the slime from the atlas.
            slimeAnimation = atlas.CreateAnimatedSprite("slime-animation");
            slimeAnimation.Scale = new Vector2(4.0f, 4.0f);
        }

        public override void Initialize()
        {
            base.Initialize();
            position = new Vector2(Core.GraphicsDevice.Viewport.Width * 0.5f, Core.GraphicsDevice.Viewport.Height * 0.5f);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            slimeAnimation.Update(gameTime);
            HandleInput(gameTime);
        }

        private void HandleInput(GameTime gameTime)
        {
            if (GameController.MoveUp())
                velocity.Y = -1;
            else if (GameController.MoveDown())
                velocity.Y = 1;
            else
                velocity.Y = 0;

            if (GameController.MoveLeft())
                velocity.X = -1;
            else if (GameController.MoveRight())
                velocity.X = 1;
            else
                velocity.X = 0;

            position += velocity.Normalized * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            slimeAnimation.Draw(Core.SpriteBatch, position);
            Core.SpriteBatch.End();

            Core.ImGuiRenderer.BeforeLayout(gameTime);
            ImGui.Begin("Slime");
            System.Numerics.Vector2 v = position.ToNumerics();
            System.Numerics.Vector2 v2 = velocity.ToNumerics();

            ImGui.DragFloat2("Position", ref v);
            ImGui.InputFloat2("Velocity", ref v2);
            ImGui.End();
            Core.ImGuiRenderer.AfterLayout();
        }
    }
}
