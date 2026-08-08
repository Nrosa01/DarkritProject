using Darkrit.Editor.Windows;
using Darkrit.InputSystem;
using Microsoft.Xna.Framework;

namespace Darkrit.Editor;

/// <summary>
/// Class that contains all editor ImGui overlays except for CoreStats
/// </summary>
internal class EditorOverlay
{
    private readonly ImGuiConsoleWindow _consoleWindow;
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
