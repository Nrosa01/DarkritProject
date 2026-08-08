// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Hexa.NET.ImGui;

namespace Darkrit.ImGuiUtils;

/// <summary>
/// Extension utils for ImGui
/// </summary>
public static class ImGuiEx
{
    /// <summary>
    /// Buttons that appear disabled based on <paramref name="isDisabled"/>
    /// </summary>
    /// <param name="label"></param>
    /// <param name="isDisabled"></param>
    /// <returns></returns>
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
