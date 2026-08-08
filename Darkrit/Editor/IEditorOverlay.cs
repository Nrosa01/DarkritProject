using Microsoft.Xna.Framework;

namespace Darkrit.Editor;

/// <summary>
/// Drawing interface for editor stuff
/// </summary>
internal interface IEditorOverlay
{
    public void Draw(GameTime gameTime);
}
