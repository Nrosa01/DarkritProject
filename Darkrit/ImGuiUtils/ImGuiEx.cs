using Hexa.NET.ImGui;

namespace Darkrit.ImGuiUtils;

/// <summary>
/// Extension utils for ImGui
/// </summary>
public static class ImGuiEx
{
    public static bool DisableButton(string label, bool isDisabled)
    {
        if (isDisabled)
            ImGui.BeginDisabled(true);
        bool result = ImGui.Button(label);
        if (isDisabled)
            ImGui.EndDisabled();

        return result;
    }
}
