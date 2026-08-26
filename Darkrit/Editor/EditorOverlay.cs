// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.Editor.MenuBar;
using Darkrit.Editor.Windows;
using Darkrit.InputSystem;
using Hexa.NET.ImGui;
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


    /// <summary>
    /// Draws the main title bar
    /// </summary>
    /// <param name="gameTime"></param>
    public void DrawMainBar(GameTime gameTime)
    {
        if (ImGui.BeginMainMenuBar())
        {
            FileMenuItem.Draw();
            if (ImGui.BeginMenu("View"))
            {
                EditorLayouts.Draw();

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }

        EditorModals.Draw();
    }

    public void Draw(GameTime gameTime)
    {
        _consoleWindow.Draw(gameTime);
        _sceneSwitcherWindow.Draw(gameTime);
        _inputRecorderWindow.Draw(gameTime);

        // This should go into its own module in the future
        ImGui.Begin("Darkrit Settings");
        if (ImGui.BeginTable("##Fields", 2))
        {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 120.0f);
            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.Text("Physics Rate");

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(-1);

            var physicsRate = Core.PHYSICS_TICKS_PER_SECOND;
            if (ImGui.DragInt("##PhysicsRate", ref physicsRate))
                Core.PHYSICS_TICKS_PER_SECOND = physicsRate;

            ImGui.EndTable();
        }
        ImGui.End();
    }
}
