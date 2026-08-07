using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Darkrit.DevTools.Logger.Renderers;
using Darkrit.Editor.Windows;
using Darkrit.ImGuiUtils;
using Darkrit.InputSystem;
using Darkrit.InputSystem.Providers;
using Darkrit.Scenes;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;

namespace Darkrit.Editor
{
    internal class EditorOverlay
    {
        private readonly ConsoleWindow _consoleWindow;
        private readonly SceneSwitcherWindow _sceneSwitcherWindow;
        private readonly InputRecorderWindow _inputRecorderWindow;

        public EditorOverlay(InputRecordingController recording)
        {
            _consoleWindow = new();
            _sceneSwitcherWindow = new();
            _inputRecorderWindow = new(recording);
        }

        public void Draw(GameTime gameTime)
        {
            _consoleWindow.Draw(gameTime);
            _sceneSwitcherWindow.Draw(gameTime);
            _inputRecorderWindow.Draw(gameTime);
        }
    }
}
