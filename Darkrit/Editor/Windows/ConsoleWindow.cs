using System;
using System.Collections.Generic;
using System.Text;
using Darkrit.DevTools.Logger;
using Darkrit.DevTools.Logger.Renderers;
using Microsoft.Xna.Framework;

namespace Darkrit.Editor.Windows
{
    internal class ConsoleWindow : IEditorOverlay
    {
        private readonly ImGuiLoggerConsole _console;

        public ConsoleWindow()
        {
            var imguiLogger = new ImGuiLogger();

            _console = new(imguiLogger);

            Log.AddLogger(imguiLogger);
        }

        public void Draw(GameTime gameTime) => _console.Draw(gameTime);
    }
}
