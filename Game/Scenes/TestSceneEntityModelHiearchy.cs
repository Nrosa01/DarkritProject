using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Darkrit;
using Darkrit.Base;
using Darkrit.DevTools.Logger;
using Darkrit.EntityModel;
using Darkrit.Graphics;
using Darkrit.InputSystem;
using Darkrit.InputSystem.Bindings;
using Darkrit.Physics.Boxy2D;
using Darkrit.Scenes;
using Darkrit.Utilities;
using DarkritGame.Scenes;
using Gum.DataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GamepadButton = Microsoft.Xna.Framework.Input.Buttons;
using Key = Microsoft.Xna.Framework.Input.Keys;

namespace DarkritGame.Scenes;

public class TestSceneEntityModelHiearchy : Scene
{
    EntityRegistry world;
    Handle<Entity> player;
    Camera camera = new();

    public override void Initialize()
    {
        TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");

        world = new(10);
        var parent = world.CreateEntityByHandle(new StringID("Parent"));

        var childI = world.CreateEntityByHandle(parent, new StringID("Child i"));
        var childG = world.CreateEntityByHandle(parent, new StringID("Child g"));
        var childH = world.CreateEntityByHandle(childG, new StringID("Child h"));

        var childA = world.CreateEntityByHandle(parent, new StringID("Child a"));
        var childC = world.CreateEntityByHandle(childA, new StringID("Child c"));
        var childF = world.CreateEntityByHandle(childC, new StringID("Child f"));
        var childD = world.CreateEntityByHandle(childC, new StringID("Child d"));
        var childE = world.CreateEntityByHandle(childD, new StringID("Child e"));
        var childB = world.CreateEntityByHandle(childA, new StringID("Child b"));
    }

    public override void Update(GameTime gameTime) => world.Update(gameTime);

    public override void FixedUpdate(GameTime gameTime) => world.FixedUpdate(gameTime);

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.GetViewMatrix(Core.Viewport), rasterizerState: RasterizerState.CullNone);
        world.Draw(gameTime);
        Core.SpriteBatch.End();

        base.Draw(gameTime);
    }

    public override void EditorDraw(GameTime gameTime)
    {
        base.EditorDraw(gameTime);
        camera.EditorDraw();
        world.EditorDraw();
    }

    public override void Deinitialize() { }
}
