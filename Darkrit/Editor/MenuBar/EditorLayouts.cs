using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Hexa.NET.ImGui;

namespace Darkrit.Editor.MenuBar;

internal class EditorLayouts
{
    private static readonly string LayoutDirectory =
        Path.Combine(
            EditorData.UserDirectory,
            "Layouts");

    /// <summary>
    /// Gets all the layout stored in the global prefs
    /// </summary>
    /// <returns></returns>
    public static string[] GetLayouts()
    {
        if (!Directory.Exists(LayoutDirectory))
            return [];

        return [.. Directory
            .GetFiles(LayoutDirectory, "*.ini")
            .Select(Path.GetFileNameWithoutExtension)];
    }

    /// <summary>
    /// Saves the current layout with <paramref name="name"/> name
    /// </summary>
    /// <param name="name"></param>
    public static void Save(string name)
    {
        Directory.CreateDirectory(LayoutDirectory);

        nuint size;

        unsafe
        {
            byte* data = ImGui.SaveIniSettingsToMemory(&size);

            if (data == null || size == 0)
                return;

            string path = Path.Combine(LayoutDirectory, $"{name}.ini");

            byte[] buffer = new byte[(int)size];

            Marshal.Copy((IntPtr)data, buffer, 0, buffer.Length);

            File.WriteAllBytes(path, buffer);
        }
    }

    /// <summary>
    /// Loads the layout named <paramref name="name"/>
    /// </summary>
    public static bool Load(string name)
    {
        string path = Path.Combine(LayoutDirectory, $"{name}.ini");

        if (!File.Exists(path))
            return false;

        byte[] data = File.ReadAllBytes(path);

        unsafe
        {
            fixed (byte* ptr = data)
            {
                ImGui.LoadIniSettingsFromMemory(ptr, (nuint)data.Length);
            }
        }

        return true;
    }

    /// <summary>
    /// Deletes a layout named <paramref name="name"/>
    /// </summary>
    public static bool Delete(string name)
    {
        string path = Path.Combine(LayoutDirectory, $"{name}.ini");

        if (!File.Exists(path))
            return false;

        File.Delete(path);

        return true;
    }

    private static string _layoutName = string.Empty;
    private static string _layoutToDelete = string.Empty;

    public static void Draw()
    {
        if (ImGui.BeginMenu("Layouts"))
        {
            foreach (string layout in GetLayouts())
            {
                ImGui.PushID(layout);

                float deleteButtonWidth = 20.0f;
                float spacing = ImGui.GetStyle().ItemSpacing.X;

                float selectableWidth = ImGui.GetContentRegionAvail().X - deleteButtonWidth - spacing;

                if (ImGui.Selectable(layout, false, ImGuiSelectableFlags.None, new System.Numerics.Vector2(selectableWidth, 0)))
                    Load(layout);

                ImGui.SameLine();

                if (ImGui.SmallButton("X"))
                {
                    _layoutToDelete = layout;

                    EditorModals.Open("Delete Layout", DrawDeleteLayoutModal);
                }

                ImGui.PopID();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Save Current Layout"))
            {
                _layoutName = string.Empty;

                EditorModals.Open("Save Layout", DrawSaveLayoutModal);
            }

            ImGui.EndMenu();
        }
    }

    private static void DrawSaveLayoutModal()
    {
        ImGui.Text("Layout name:");

        ImGui.InputText("##LayoutName", ref _layoutName, 128);

        ImGui.Spacing();

        if (ImGui.Button("Save"))
        {
            if (!string.IsNullOrWhiteSpace(_layoutName))
            {
                Save(_layoutName.Trim());
                EditorModals.Close();
            }
        }

        ImGui.SameLine();

        if (ImGui.Button("Cancel"))
            EditorModals.Close();
    }

    private static void DrawDeleteLayoutModal()
    {
        ImGui.Text($"Delete '{_layoutToDelete}'?");

        ImGui.Spacing();

        if (ImGui.Button("Delete"))
        {
            Delete(_layoutToDelete);

            _layoutToDelete = string.Empty;

            EditorModals.Close();
        }

        ImGui.SameLine();

        if (ImGui.Button("Cancel"))
            EditorModals.Close();
    }
}
