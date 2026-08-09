using System;
using System.Collections.Generic;
using System.Text;
using Hexa.NET.ImGui;

namespace Darkrit.Editor.MenuBar;

internal class FileMenuItem
{
    private static string _editorDataName = string.Empty;

    /// <summary>
    /// Draws the File Menu Item part of the ImGui MainMenuBar
    /// 
    /// Right now it has hardcoded the Export and Import editor data that
    /// is just a wrapper around <see cref="EditorData"/> controller
    /// </summary>
    public static void Draw()
    {
        if (ImGui.BeginMenu("File"))
        {
            // Exports the data as zip to a harcoded location that is SolutionRoot/.darkrit
            if (ImGui.MenuItem("Export Editor Data"))
            {
                EditorModals.Open("Export Data", static () =>
                {
                    ImGui.Text("Export Editor Data");

                    ImGui.InputText("Name", ref _editorDataName, 128);

                    ImGui.Spacing();

                    if (ImGui.Button("Export"))
                    {
                        if (!string.IsNullOrWhiteSpace(_editorDataName))
                        {
                            EditorData.Export(_editorDataName.Trim());
                            EditorModals.Close();
                        }
                    }

                    ImGui.SameLine();

                    if (ImGui.Button("Cancel"))
                        EditorModals.Close();
                });
            }

            // Reads from SolutionRoot/.darkrit to father all exported packs and allows to import them
            if (ImGui.MenuItem("Import Editor Data"))
            {
                EditorModals.Open("Import Data", static () =>
                {
                    ImGui.Text("Import Editor Data");

                    foreach (string pack in EditorData.GetAvailablePacks())
                    {
                        if (ImGui.Selectable(pack))
                        {
                            EditorData.Import(pack);
                            EditorModals.Close();
                        }
                    }

                    ImGui.Spacing();

                    if (ImGui.Button("Cancel"))
                        EditorModals.Close();
                });
            }

            ImGui.EndMenu();
        }
    }
}
