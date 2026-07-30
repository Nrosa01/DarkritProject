using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Darkrit.ImGuiUtils.Themes
{
    internal class PurpleComfyTheme
    {
        public static void SetupImGuiStyle()
        {
            // Purple Comfy styleRegularLunar from ImThemes
            var style = ImGuiNET.ImGui.GetStyle();

            style.Alpha = 1.0f;
            style.DisabledAlpha = 0.1f;
            style.WindowPadding = new Vector2(8.0f, 8.0f);
            style.WindowRounding = 10.0f;
            style.WindowBorderSize = 0.0f;
            style.WindowMinSize = new Vector2(30.0f, 30.0f);
            style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Right;
            style.ChildRounding = 5.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 10.0f;
            style.PopupBorderSize = 0.0f;
            style.FramePadding = new Vector2(5.0f, 3.5f);
            style.FrameRounding = 5.0f;
            style.FrameBorderSize = 0.0f;
            style.ItemSpacing = new Vector2(5.0f, 4.0f);
            style.ItemInnerSpacing = new Vector2(5.0f, 5.0f);
            style.CellPadding = new Vector2(4.0f, 2.0f);
            style.IndentSpacing = 5.0f;
            style.ColumnsMinSpacing = 5.0f;
            style.ScrollbarSize = 15.0f;
            style.ScrollbarRounding = 9.0f;
            style.GrabMinSize = 15.0f;
            style.GrabRounding = 5.0f;
            style.TabRounding = 5.0f;
            style.TabBorderSize = 0.0f;
            style.TabMinWidthForCloseButton = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(1.0f, 1.0f, 1.0f, 0.360515f);
            style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.09803922f, 0.09803922f, 0.09803922f, 1.0f);
            style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.09803922f, 0.09803922f, 0.09803922f, 1.0f);
            style.Colors[(int)ImGuiCol.Border] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.15686275f, 0.15686275f, 0.15686275f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.38039216f, 0.42352942f, 0.57254905f, 0.54901963f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.09803922f, 0.09803922f, 0.09803922f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.09803922f, 0.09803922f, 0.09803922f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.25882354f, 0.25882354f, 0.25882354f, 0.0f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.15686275f, 0.15686275f, 0.15686275f, 0.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.15686275f, 0.15686275f, 0.15686275f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.23529412f, 0.23529412f, 0.23529412f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.29411766f, 0.29411766f, 0.29411766f, 1.0f);
            style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.Button] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            //style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            //style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.0f, 0.4509804f, 1.0f, 0.0f);
            //style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.13333334f, 0.25882354f, 0.42352942f, 0.0f);
            style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.29411766f, 0.29411766f, 0.29411766f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.7372549f, 0.69411767f, 0.8862745f, 0.54901963f);
            style.Colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.1882353f, 0.1882353f, 0.2f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.2901961f);
            style.Colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.0f, 1.0f, 1.0f, 0.03433478f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.5019608f, 0.3019608f, 1.0f, 0.54901963f);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.0f, 1.0f, 0.0f, 0.9f);
            //style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.0f, 1.0f, 1.0f, 0.7f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.8f, 0.8f, 0.8f, 0.2f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.8f, 0.8f, 0.8f, 0.35f);
        }
    }
}
