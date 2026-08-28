using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Darkrit;
using Darkrit.Base;
using Darkrit.DevTools.Logger;
using Darkrit.EntityModel;
using Darkrit.EntityModel.Components;
using Darkrit.Graphics;
using Darkrit.Physics.Boxy2D;
using Darkrit.Scenes;
using DarkritGame.Scenes;
using Game.Components;
using Gum.DataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DarkritGame.Scenes;

[Component]
public partial struct SpriteComponent
{
    public AnimatedSprite Sprite;

    [SerializeField] Vector2 pivot = Vector2.One * 0.5f;
    [SerializeField] Vector2 offset;

    public Vector2 Pivot
    {
        readonly get => pivot;
        set
        {
            Vector2 delta = (value - pivot) * Sprite.Region.Size;
            pivot = value;

            Entity.Position += TransformLocal(delta);
            UpdateOrigin();
        }
    }

    public Vector2 Offset
    {
        readonly get => offset;
        set
        {
            Vector2 delta = value - offset;
            offset = value;

            Entity.Position -= TransformLocal(delta);
        }
    }

    [Button]
    readonly void UpdateOrigin()
    {
        Sprite.Origin = pivot * new Vector2(Sprite.Region.Width, Sprite.Region.Height);
    }

    readonly Vector2 TransformLocal(Vector2 value)
    {
        value *= Entity.Scale;

        return Vector2.Transform(value, Matrix.CreateRotationZ(Entity.Rotation));
    }

    public readonly void OnAdd()
    {
        Entity.Scale = Vector2.One * 2;
        UpdateOrigin();
    }

    public readonly void FixedUpdate(GameTime gameTime)
    {
        Sprite.Update(gameTime);
    }

    public readonly void Draw(GameTime gameTime)
    {
        Vector2 drawPosition = Entity.Position + TransformLocal(offset);

        Sprite.Draw(
            Core.SpriteBatch,
            drawPosition,
            Entity.Scale,
            Entity.Rotation);

        Vector2 pivotSize = Vector2.One * 4f;

        Core.SpriteBatch.Draw(
            Core.Pixel,
            Entity.Position - pivotSize * 0.5f,
            null,
            Color.Red,
            0f,
            Vector2.Zero,
            pivotSize,
            SpriteEffects.None,
            1f);
    }
}

[Component]
[InjectComponent(typeof(SquareRenderer))]
public partial struct Mover
{
    static readonly int WindowsWidth = Core.GraphicsDevice.Viewport.Width;
    static readonly int WindowsHeight = Core.GraphicsDevice.Viewport.Height;

    Handle<SquareRenderer> squareHandle;
    ComponentStore<SquareRenderer> store;

    public Vector2 Velocity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(GameTime gameTime)
    {
        var size = SquareRenderer.Size;

        Entity.Position += Velocity;

        if (Entity.Position.X < 0 || Entity.Position.X + size > WindowsWidth)
            Velocity.X *= -1;

        if (Entity.Position.Y < 0 || Entity.Position.Y + size > WindowsHeight)
            Velocity.Y *= -1;
    }
}

[Component]
public partial struct SquareRenderer
{
    public int Size;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Draw(GameTime gameTime)
    {
        var pos = Entity.Position;
        float r = pos.X - MathF.Floor(pos.X);
        float g = pos.Y - MathF.Floor(pos.Y);

        var color = new Color(r, g, pos.X);
        Core.SpriteBatch.Draw(Core.Pixel, pos, null, color, Entity.Rotation, Vector2.Zero, Entity.Scale * Size, SpriteEffects.None, 0);
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
        ref Entity playerRef = ref entityWorld.CreateEntity();
        player = playerRef.Handle;
        playerRef.Name = "Player";
        ref var sprite = ref playerRef.AddComponent<SpriteComponent>(new SpriteComponent
        {
            Sprite = atlas.CreateAnimatedSprite("slime-animation")
        });
        playerRef.AddComponent<PlayerController>();
        ref var physics = ref playerRef.AddComponent<PhysicsBody>();
        physics.Size = sprite.Sprite.Size;

        ref Entity square = ref entityWorld.CreateEntity(player, "Square");
        square.Position += new Vector2(0, 100);
        ref var physicsSquare = ref square.AddComponent<PhysicsBody>();
        physicsSquare.Size = new Vector2(2000, 10);

        square = ref entityWorld.CreateEntity(player, "Square 2");
        square.Position += new Vector2(0, -30);
        physicsSquare = ref square.AddComponent<PhysicsBody>();
        physicsSquare.Size = new Vector2(200, 10);
        physicsSquare.IsOneWay = true;
        square.AddComponent<MovingPlatform>();

        square = ref entityWorld.CreateEntity(player, "Square 3");
        square.Position += new Vector2(-180, 50);
        physicsSquare = ref square.AddComponent<PhysicsBody>();
        physicsSquare.Size = new Vector2(20, 100);
    }

    public override void Update(GameTime gameTime) => entityWorld.Update(gameTime);

    public override void FixedUpdate(GameTime gameTime) => entityWorld.FixedUpdate(gameTime);
    public override void LateUpdate(GameTime gameTime) => entityWorld.LateUpdate(gameTime);

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
        entityWorld.EditorDraw();
    }

    public override void Deinitialize() { }
}
