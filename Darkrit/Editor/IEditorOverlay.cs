using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Darkrit.Editor
{
    internal interface IEditorOverlay
    {
        public void Draw(GameTime gameTime);
    }
}
