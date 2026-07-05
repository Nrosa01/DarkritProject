using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;

namespace DungeonSlime.Scenes
{
    internal class TestScene : Scene
    {
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));
        }
    }
}
