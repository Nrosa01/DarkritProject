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
using MonoGameLibrary.TinyECS;
using Console = MonoGameLibrary.Utilities.Log;

namespace DungeonSlime.Scenes
{
    internal class TestScene : Scene
    {
        AnimatedSprite slimeAnimation;
        Vector2 position;
        Vector2 velocity;
        World world;
        private float speed = 500f;

        record struct Square(int Size);

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
    
            world = new World(5000);

            for (var i = 0; i < 3000; i++)
            {
                var spacing = .2f;
                var entity = world.Create();
                world.AddComponent(entity, new Position { X = (i + 1) * spacing, Y = (i + 1) * spacing });
                world.AddComponent(entity, new Velocity { X = 8 + i * 0.01f, Y = 4f + i * 0.01f });
                world.AddComponent(entity, new Square { Size = 10 });

                if (i % 2 == 0) world.AddComponent(entity, new Fart { Power = 666 });
            }
        }

        static void RunVelocitySystem(World registry)
        {
            var view = registry.View<Velocity, Position, Square>();
            var WindowsWidth = Core.GraphicsDevice.Viewport.Width;
            var WindowsHeight = Core.GraphicsDevice.Viewport.Height;
            foreach (var entity in view)
            {
                ref Position pos = ref registry.GetComponent<Position>(entity);
                ref Velocity vel = ref registry.GetComponent<Velocity>(entity);
                ref Square square = ref registry.GetComponent<Square>(entity);
                pos.X += vel.X;
                pos.Y += vel.Y;

                if (pos.X < 0 || pos.X + square.Size > WindowsWidth)
                    vel.X *= -1;

                if (pos.Y < 0 || pos.Y + square.Size > WindowsHeight)
                    vel.Y *= -1;

            }
        }

        static void RunSquareSystem(World registry, SpriteBatch spritebatch)
        {
            Console.WriteLine("----- Printer -----");
            var view = registry.View<Position, Square>();
            foreach (var entity in view)
            {
                var pos = registry.GetComponent<Position>(entity);
                var square = registry.GetComponent<Square>(entity);

                spritebatch.Draw(Core.Pixel, new Rectangle((int)pos.X, (int)pos.Y, square.Size, square.Size), null, Color.Wheat);
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
            RunVelocitySystem(world);
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

        public override void DebugDraw(GameTime gameTime)
        {
            ImGui.Begin("Slime");
            System.Numerics.Vector2 v = position.ToNumerics();
            System.Numerics.Vector2 v2 = velocity.ToNumerics();

            ImGui.DragFloat2("Position", ref v);
            ImGui.InputFloat2("Velocity", ref v2);
            ImGui.End();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            slimeAnimation.Draw(Core.SpriteBatch, position);
            RunSquareSystem(world, Core.SpriteBatch);
            Core.SpriteBatch.End();
        }
    }
}
