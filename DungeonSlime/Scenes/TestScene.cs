using System;
using System.Collections.Generic;
using System.Text;
using ImGuiNET;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.Utilities;
using DungeonSlime.TinyECS;

namespace DungeonSlime.Scenes
{
    internal class TestScene : Scene
    {
        AnimatedSprite slimeAnimation;
        Vector2 position;
        Vector2 velocity;
        private float speed = 500f;

        record struct Position(float X, float Y);
        record struct Velocity(float X, float Y);
        record struct Fart(int Power);

        public override void LoadContent()
        {
            base.LoadContent();
            // Create the texture atlas from the XML configuration file.
            TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");

            // Create the animated sprite for the slime from the atlas.
            slimeAnimation = atlas.CreateAnimatedSprite("slime-animation");
            slimeAnimation.Scale = new Vector2(4.0f, 4.0f);
    
            var registry = new World(100);

            for (var i = 0; i < 20; i++)
            {
                Int32 entity = registry.Create();
                registry.AddComponent<Position>(entity, new Position { X = i * 10, Y = i * 10 });
                registry.AddComponent<Velocity>(entity, new Velocity { X = 2, Y = 2 });

                if (i % 5 == 0) registry.AddComponent<Fart>(entity, new Fart { Power = 666 });
            }

            RunPrinterSystem(registry);
            RunVelocitySystem(registry);

            RunPrinterSystem(registry);
            RunVelocitySystem(registry);
        }

        static void RunVelocitySystem(World registry)
        {
            var view = registry.View<Velocity, Position>();
            foreach (var entity in view)
            {
                ref Position pos = ref registry.GetComponent<Position>(entity);
                ref Velocity vel = ref registry.GetComponent<Velocity>(entity);
                pos.X += vel.X;
                pos.Y += vel.Y;
            }
        }

        static void RunPrinterSystem(World registry)
        {
            Console.WriteLine("----- Printer -----");
            var view = registry.View<Velocity, Position, Fart>();
            foreach (var entity in view)
            {
                var pos = registry.GetComponent<Position>(entity);
                Console.WriteLine($"entity: {entity}, pos: {pos.X},{pos.Y}");
            }
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
