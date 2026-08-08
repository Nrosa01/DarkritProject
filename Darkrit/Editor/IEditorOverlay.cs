// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;

namespace Darkrit.Editor;

/// <summary>
/// Drawing interface for editor stuff
/// </summary>
internal interface IEditorOverlay
{
    public void Draw(GameTime gameTime);
}
