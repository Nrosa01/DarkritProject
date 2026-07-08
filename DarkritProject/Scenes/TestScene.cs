using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Frent;
using Frent.Core;
using Frent.Systems;
using Frent.Updating;
using ImGuiNET;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.TinyECS;
using MonoGameLibrary.Utilities;
using Console = MonoGameLibrary.Utilities.Log;


namespace Darkrit.Scenes
{
    internal class TestScene : Scene
    {
        static bool USE_FREN = false;

        enum FrentMode
        {
            Delegate, Inline, Parallel, Enumerate
        }

        enum TinyECSMode
        {
            Delegate,
            DelegateParallel,
            Enumerate
        }

        static FrentMode frentMode = FrentMode.Inline;
        static TinyECSMode tinyEcsMode = TinyECSMode.DelegateParallel;
        readonly string[] frentNames = Enum.GetNames<FrentMode>();
        readonly string[] tinyNames = Enum.GetNames<TinyECSMode>();
        static bool paused = true;
        static bool render = false;

        const int worldSize = 1_000_000;


        AnimatedSprite slimeAnimation;
        Vector2 position;
        Vector2 velocity;
        MonoGameLibrary.TinyECS.Registry world;
        Frent.World frentWorld;
        private float speed = 500f;

        static int WindowsWidth = Core.GraphicsDevice.Viewport.Width;
        static int WindowsHeight = Core.GraphicsDevice.Viewport.Height;

        private static readonly EntityType _entityType = EntityType.EntityTypeOf([Component<Position>.ID, Component<Velocity>.ID, Component<Square>.ID], []);

        record struct Square(int Size);

        record struct Position(float X, float Y);
        record struct Velocity(float X, float Y);
        record struct Fart(int Power);

        public override void Initialize()
        {
            position = new Vector2(Core.GraphicsDevice.Viewport.Width * 0.5f, Core.GraphicsDevice.Viewport.Height * 0.5f);

            // Create the texture atlas from the XML configuration file.
            TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");

            // Create the animated sprite for the slime from the atlas.
            slimeAnimation = atlas.CreateAnimatedSprite("slime-animation");
            slimeAnimation.Scale = new Vector2(4.0f, 4.0f);


            if (USE_FREN)
            {
                frentWorld = new();
                frentWorld.EnsureCapacity(_entityType, worldSize);

                for (int i = 0; i < worldSize; i++)
                {
                    var spacing = .2f;


                    var entity = frentWorld.Create(new Position { X = (i + 1) * spacing, Y = (i + 1) * spacing }, new Velocity { X = 8 + i * 0.01f, Y = 4f + i * 0.01f }, new Square { Size = 10 });
                    if (i % 2 == 0) entity.Add(new Fart { Power = 666 });
                }
            }
            else
            {
                world = new MonoGameLibrary.TinyECS.Registry(worldSize);

                for (var i = 0; i < worldSize; i++)
                {
                    var spacing = .2f;
                    var entity = world.Create();
                    world.AddComponent(entity, new Position { X = (i + 1) * spacing, Y = (i + 1) * spacing });
                    world.AddComponent(entity, new Velocity { X = 8 + i * 0.01f, Y = 4f + i * 0.01f });
                    world.AddComponent(entity, new Square { Size = 10 });

                    if (i % 2 == 0) world.AddComponent(entity, new Fart { Power = 666 });
                }
            }
        }

        struct InlineRun : IAction<Velocity, Position, Square>
        {
            public readonly void Run(ref Velocity vel, ref Position pos, ref Square square)
            {
                pos.X += vel.X;
                pos.Y += vel.Y;

                if (pos.X < 0 || pos.X + square.Size > WindowsWidth)
                    vel.X *= -1;

                if (pos.Y < 0 || pos.Y + square.Size > WindowsHeight)
                    vel.Y *= -1;
            }
        }

        static void RunVelocitySystem(MonoGameLibrary.TinyECS.Registry registry, Frent.World frentWorld)
        {
            if (USE_FREN)
            {
                if (frentMode == FrentMode.Inline)
                {
                    frentWorld.Query<Velocity, Position, Square>().Inline<InlineRun, Velocity, Position, Square>(default);
                }
                else if (frentMode == FrentMode.Enumerate)
                {
                    foreach (var comp in frentWorld.Query<Velocity, Position, Square>().Enumerate<Velocity, Position, Square>())
                    {

                        comp.Item1.Value.X += comp.Item2.Value.X;
                        comp.Item1.Value.Y += comp.Item2.Value.Y;

                        if (comp.Item1.Value.X < 0 || comp.Item1.Value.X + comp.Item3.Value.Size > WindowsWidth)
                            comp.Item2.Value.X *= -1;

                        if (comp.Item1.Value.Y < 0 || comp.Item1.Value.Y + comp.Item3.Value.Size > WindowsHeight)
                            comp.Item2.Value.Y *= -1;
                    }
                }
                else if (frentMode == FrentMode.Parallel)
                {

                }
                else
                {
                    frentWorld.Query<Velocity, Position, Square>().Delegate((ref Velocity vel, ref Position pos, ref Square square) =>
                    {
                        pos.X += vel.X;
                        pos.Y += vel.Y;

                        if (pos.X < 0 || pos.X + square.Size > WindowsWidth)
                            vel.X *= -1;

                        if (pos.Y < 0 || pos.Y + square.Size > WindowsHeight)
                            vel.Y *= -1;
                    });
                }
            }
            else
            {
                bool runParallel = tinyEcsMode == TinyECSMode.DelegateParallel;
                switch (tinyEcsMode)
                {
                    case TinyECSMode.DelegateParallel:
                    case TinyECSMode.Delegate:
                        registry.Query((ref Velocity vel, ref Position pos, ref Square square) =>
                        {
                            pos.X += vel.X;
                            pos.Y += vel.Y;

                            if (pos.X < 0 || pos.X + square.Size > WindowsWidth)
                                vel.X *= -1;

                            if (pos.Y < 0 || pos.Y + square.Size > WindowsHeight)
                                vel.Y *= -1;
                        }, runParallel);
                        break;
                    case TinyECSMode.Enumerate:
                        var view = registry.View<Velocity, Position, Square>();
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
                        break;
                }
            }
        }

        struct DrawInline : IAction<Position, Square>
        {
            public readonly void Run(ref Position pos, ref Square square)
            {
                Core.SpriteBatch.Draw(Core.Pixel, new Rectangle((int)pos.X, (int)pos.Y, square.Size, square.Size), null, Color.Wheat);
            }
        }

        static void RunSquareSystem(MonoGameLibrary.TinyECS.Registry registry, SpriteBatch spritebatch, Frent.World frentWorld)
        {
            if (USE_FREN)
            {
                if (frentMode == FrentMode.Inline)
                {
                    frentWorld.Query<Position, Square>().Inline<DrawInline, Position, Square>(default);
                }
                else
                {
                    frentWorld.Query<Position, Square>().Delegate((ref Position pos, ref Square square) =>
                    {
                        spritebatch.Draw(Core.Pixel, new Rectangle((int)pos.X, (int)pos.Y, square.Size, square.Size), null, Color.Wheat);
                    });
                }
            }
            else
            {

                switch (tinyEcsMode)
                {
                    case TinyECSMode.Delegate:
                    case TinyECSMode.DelegateParallel: // Graphics can't run in parallel
                        registry.Query<Position, Square>((ref Position pos, ref Square square) => {
                            spritebatch.Draw(Core.Pixel, new Rectangle((int)pos.X, (int)pos.Y, square.Size, square.Size), null, Color.Wheat);
                        });
                        break;
                    case TinyECSMode.Enumerate:
                        var view = registry.View<Position, Square>();
                        foreach (var entity in view)
                        {
                            var pos = registry.GetComponent<Position>(entity);
                            var square = registry.GetComponent<Square>(entity);

                            spritebatch.Draw(Core.Pixel, new Rectangle((int)pos.X, (int)pos.Y, square.Size, square.Size), null, Color.Wheat);
                        }
                        break;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (paused) return;

            slimeAnimation.Update(gameTime);
            HandleInput(gameTime);
            RunVelocitySystem(world, frentWorld);
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
            ImGui.Begin("Test");
            ImGui.Checkbox("Render", ref render);
            if(ImGui.Checkbox("Use Frent", ref USE_FREN))
            {
                if(!USE_FREN && world == null)
                {
                    world = new MonoGameLibrary.TinyECS.Registry(worldSize);

                    for (var i = 0; i < worldSize; i++)
                    {
                        var spacing = .2f;
                        var entity = world.Create();
                        world.AddComponent(entity, new Position { X = (i + 1) * spacing, Y = (i + 1) * spacing });
                        world.AddComponent(entity, new Velocity { X = 8 + i * 0.01f, Y = 4f + i * 0.01f });
                        world.AddComponent(entity, new Square { Size = 10 });

                        if (i % 2 == 0) world.AddComponent(entity, new Fart { Power = 666 });
                    }
                }
                else if(frentWorld == null)
                {
                    frentWorld = new();
                    frentWorld.EnsureCapacity(_entityType, worldSize);

                    for (int i = 0; i < worldSize; i++)
                    {
                        var spacing = .2f;


                        var entity = frentWorld.Create(new Position { X = (i + 1) * spacing, Y = (i + 1) * spacing }, new Velocity { X = 8 + i * 0.01f, Y = 4f + i * 0.01f }, new Square { Size = 10 });
                        if (i % 2 == 0) entity.Add(new Fart { Power = 666 });
                    }
                }
            }

            if (USE_FREN)
                ImGui.BeginDisabled();
            int current = (int)tinyEcsMode;
            if (ImGui.Combo("TinyECSmODE", ref current, tinyNames, tinyNames.Length))
            {
                tinyEcsMode = (TinyECSMode)current;
            }
            if (USE_FREN)
                ImGui.EndDisabled();

            ImGui.Checkbox("Pause", ref paused);
            if (!USE_FREN)
                ImGui.BeginDisabled();
            current = (int)frentMode;
            if (ImGui.Combo("FrentMode", ref current, frentNames, frentNames.Length))
            {
                frentMode = (FrentMode)current;
            }
            if (!USE_FREN)
                ImGui.EndDisabled();

            ImGui.End();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            slimeAnimation.Draw(Core.SpriteBatch, position);
            if (render)
                RunSquareSystem(world, Core.SpriteBatch, frentWorld);
            Core.SpriteBatch.End();
        }

        public override void Deinitialize()
        {
        }
    }
}
