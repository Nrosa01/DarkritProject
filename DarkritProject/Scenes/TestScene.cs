using Frent;
using Frent.Core;
using Frent.Systems;
using Frent.Updating;
using Guilred.Rendering;
using ImGuiNET;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.TinyECS;
using MonoGameLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Console = MonoGameLibrary.Utilities.Log;


namespace Darkrit.Scenes
{
    internal class TestScene : Scene
    {
        static GuilBatch batcher;

        static bool USE_FREN = false;

        enum FrentMode
        {
            Delegate, Inline, Enumerate
        }

        enum TinyECSMode
        {
            Delegate,
            DelegateParallel,
        }

        static FrentMode frentMode = FrentMode.Inline;
        static TinyECSMode tinyEcsMode = TinyECSMode.DelegateParallel;
        readonly string[] frentNames = Enum.GetNames<FrentMode>();
        readonly string[] tinyNames = Enum.GetNames<TinyECSMode>();
        static bool paused = true;
        static bool render = false;
        static bool useGuildRender = false;

        const int worldSize = 10_000;


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
            batcher = new GuilBatch(Core.GraphicsDevice);

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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void UpdateAction(ref Velocity vel, ref Position pos, ref Square square)
        {
            pos.X += vel.X;
            pos.Y += vel.Y;

            if (pos.X < 0 || pos.X + square.Size > WindowsWidth)
                vel.X *= -1;

            if (pos.Y < 0 || pos.Y + square.Size > WindowsHeight)
                vel.Y *= -1;
        }

        struct InlineRun : IAction<Velocity, Position, Square>
        {
            public readonly void Run(ref Velocity vel, ref Position pos, ref Square square) => UpdateAction(ref vel, ref pos, ref square);
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
                        UpdateAction(ref comp.Item1.Value, ref comp.Item2.Value, ref comp.Item3.Value);
                }
                else
                {
                    frentWorld.Query<Velocity, Position, Square>().Delegate<Velocity, Position, Square>(UpdateAction);
                }
            }
            else
            {
                switch (tinyEcsMode)
                {
                    case TinyECSMode.DelegateParallel:
                        registry.QueryParallel<Velocity, Position, Square>(UpdateAction);
                        break;
                    case TinyECSMode.Delegate:
                            registry.Query<Velocity, Position, Square>(UpdateAction);
                        break;
                }
            }
        }

        struct DrawInline : IAction<Position, Square>
        {
            public readonly void Run(ref Position pos, ref Square square)
            {
                if(useGuildRender)
                    batcher.DrawTexture(Core.Pixel, new Rectangle((int)pos.X, (int)pos.Y, square.Size, square.Size), null, Color.Wheat);
                else
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
                        if (useGuildRender)
                            batcher.DrawTexture(Core.Pixel, new Rectangle((int)pos.X, (int)pos.Y, square.Size, square.Size), null, Color.Wheat);
                        else
                            Core.SpriteBatch.Draw(Core.Pixel, new Rectangle((int)pos.X, (int)pos.Y, square.Size, square.Size), null, Color.Wheat);
                    });
                }
            }
            else
            {

                switch (tinyEcsMode)
                {
                    case TinyECSMode.Delegate:
                    case TinyECSMode.DelegateParallel: // Graphics can't run in parallel
                        registry.Query<Position, Square>((ref Position pos, ref Square square) =>
                        {
                            if (useGuildRender)
                                batcher.DrawTexture(Core.Pixel, new Rectangle((int)pos.X, (int)pos.Y, square.Size, square.Size), null, Color.Wheat);
                            else
                                Core.SpriteBatch.Draw(Core.Pixel, new Rectangle((int)pos.X, (int)pos.Y, square.Size, square.Size), null, Color.Wheat);
                        });
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
            ImGui.Checkbox("Guild Render", ref useGuildRender);
            if (ImGui.Checkbox("Use Frent", ref USE_FREN))
            {
                if (!USE_FREN && world == null)
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
                else if (frentWorld == null)
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

            if(useGuildRender)
                batcher.Begin();
            else
                Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);


            if (render)
                RunSquareSystem(world, Core.SpriteBatch, frentWorld);

            if(!useGuildRender)
                slimeAnimation.Draw(Core.SpriteBatch, position);
            
            if(useGuildRender)
                batcher.End();
            else
                Core.SpriteBatch.End();
        }

        public override void Deinitialize()
        {
        }
    }
}
