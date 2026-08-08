// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.DevTools.Logger;
using Darkrit.DevTools.Logger.Renderers;
using Microsoft.Xna.Framework;

namespace Darkrit.Editor.Windows;

/// <summary>
/// Editor overlay wrapper over the ImGuiLoggerConsole
/// </summary>
internal class ImGuiConsoleWindow : IEditorOverlay
{
    private readonly ImGuiLoggerConsole _console;

    public ImGuiConsoleWindow()
    {
        var imguiLogger = new CompactLogger();

        _console = new(imguiLogger);

        Log.AddLogger(imguiLogger);
    }

    public void Draw(GameTime gameTime) => _console.Draw(gameTime);
}
