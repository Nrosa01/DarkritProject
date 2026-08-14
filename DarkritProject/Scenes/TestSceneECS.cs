using Darkrit.Graphics.InstancedQuadRenderer;
using Darkrit.TinyECS;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;

namespace Darkrit.Scenes;

internal class TestSceneECS : Scene
{
    enum TinyECSMode
    {
        Delegate,
        DelegateParallel,
    }

    static TinyECSMode tinyEcsMode = TinyECSMode.DelegateParallel;
    readonly string[] tinyNames = Enum.GetNames<TinyECSMode>();
    static bool paused = false;
    static bool render = true;
    static bool renderInstanced = true;

    const int worldSize = 10_000;

    TinyECS.Registry world;

    InstancedQuadRenderer instancedQuadRenderer;

    static readonly int WindowsWidth = Core.GraphicsDevice.Viewport.Width;
    static readonly int WindowsHeight = Core.GraphicsDevice.Viewport.Height;

    record struct Square(int Size) : IComponent;

    record struct Position(float X, float Y) : IComponent;
    record struct Velocity(float X, float Y) : IComponent;
    record struct Fart(int Power) : IComponent;

    public override void Initialize()
    {
        instancedQuadRenderer = new(Core.GraphicsDevice, Content);

        world = new TinyECS.Registry(worldSize);

        for (var i = 0; i < worldSize; i++)
        {
            var entity = world.Create();
            world.AddComponent(entity, new Position { X = WindowsWidth / 2, Y = WindowsHeight / 2 });
            world.AddComponent(entity, new Velocity { X = 8 + i * 0.01f, Y = 4f + i * 0.01f });
            world.AddComponent(entity, new Square { Size = 10 });

            if (i % 2 == 0) world.AddComponent(entity, new Fart { Power = 666 });
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

    static void RunVelocitySystem(TinyECS.Registry registry)
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

    static void RunSquareSystem(TinyECS.Registry registry, Action<Texture2D, Rectangle, Rectangle?, Color> drawAction)
    {
        ArgumentNullException.ThrowIfNull(registry);
        switch (tinyEcsMode)
        {
            case TinyECSMode.Delegate:
            case TinyECSMode.DelegateParallel: // Graphics can't run in parallel
                registry.Query((ref Position pos, ref Square square) =>
                {
                    float r = pos.X - MathF.Floor(pos.X);
                    float g = pos.Y - MathF.Floor(pos.Y);

                    var color = new Color(r, g, pos.X);
                    drawAction(Core.Pixel, new Rectangle((int)pos.X, (int)pos.Y, square.Size, square.Size), null, color);
                });
                break;
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (paused) return;

        RunVelocitySystem(world);
    }

    public override void EditorDraw(GameTime gameTime)
    {
        ImGui.Begin("Test");
        ImGui.Checkbox("Render", ref render);
        if (render)
            ImGui.Checkbox("RenderInstanced", ref renderInstanced);
       
        ImGui.Checkbox("Pause", ref paused);

        int current = (int)tinyEcsMode;
        if (ImGui.Combo("TinyECSmODE", ref current, tinyNames, tinyNames.Length))
        {
            tinyEcsMode = (TinyECSMode)current;
        }

        ImGui.End();
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        instancedQuadRenderer.Begin();
        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        if (render)
            RunSquareSystem(world, renderInstanced ? instancedQuadRenderer.Draw : Core.SpriteBatch.Draw);
        Core.SpriteBatch.End();
        instancedQuadRenderer.End();
    }

    public override void Deinitialize()
    {
    }
}
