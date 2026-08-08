using System;
using Microsoft.Xna.Framework;
using Hexa.NET.ImGui;

namespace Darkrit.DevTools.Logger.Renderers;

/// <summary>
/// Class that draws an ImGuiWindows with a virtualized list of logs
/// </summary>
/// <param name="logger">The logger that contains the data</param>
internal class ImGuiLoggerConsole(CompactLogger logger)
{
    [Flags]
    internal enum LogLevelFilter
    {
        None = 0,
        Trace = 1 << 0,
        Debug = 1 << 1,
        Info = 1 << 2,
        Warning = 1 << 3,
        Error = 1 << 4,

        All = Trace | Debug | Info | Warning | Error
    }

    LogLevelFilter filter = LogLevelFilter.All;

    bool collapseRepeated = true;

    string searchText = "";
    internal void Draw(GameTime gameTime)
    {
        var buffer = logger.Buffer;

        ImGui.Begin("Console");

        ImGui.SameLine();
        if (ImGui.Button("Clear"))
            buffer.Clear();

        ImGui.SameLine();
        ImGui.Text($"{buffer.Size} entries");

        ImGui.SameLine();

        ImGui.Checkbox("Collapse repeated", ref collapseRepeated);
        logger.CollapseRepeated = collapseRepeated;

        ImGui.Dummy(new Vector2(5, 0).ToNumerics());
        
        DrawLevelToggle(LogLevelFilter.Trace, "Trace");
        DrawLevelToggle(LogLevelFilter.Debug, "Debug");
        DrawLevelToggle(LogLevelFilter.Info, "Info");
        DrawLevelToggle(LogLevelFilter.Warning, "Warning");
        DrawLevelToggle(LogLevelFilter.Error, "Error");
        ImGui.NewLine();

        ImGui.InputTextWithHint("##search", "Search...", ref searchText, 256);


        ImGui.BeginChild("Logs");

        // I have to do my own clipping because ImGuiClipper isn't exposed, this gives more control anyways
        float lineHeight = ImGui.GetTextLineHeightWithSpacing();

        int total = buffer.Size;

        float scrollY = ImGui.GetScrollY();
        float windowHeight = ImGui.GetWindowHeight();

        int first = SMath.Max(0, (int)(scrollY / lineHeight));
        int visible = (int)SMath.Ceiling(windowHeight / lineHeight) + 1;
        int last = SMath.Min(total, first + visible);

        bool atBottom = ImGui.GetScrollY() >= ImGui.GetScrollMaxY() - 2.0f;

        // Space to lines that aren't drawn at the beggining
        ImGui.Dummy(new Vector2(0, first * lineHeight).ToNumerics());

        for (int i = first; i < last; i++)
        {
            var entry = buffer[i];

            if ((filter & (LogLevelFilter)(1 << (int)entry.logLevel)) == 0)
                continue;

            if (!string.IsNullOrEmpty(searchText) &&
                !entry.message.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                continue;

            DrawLog(entry);
        }

        // Space to lines that aren't drawn at the end
        ImGui.Dummy(new Vector2(0, (total - last) * lineHeight).ToNumerics());

        if (atBottom)
            ImGui.SetScrollHereY(1.0f);

        ImGui.EndChild();

        ImGui.End();
    }

    private void DrawLevelToggle(LogLevelFilter flag, string text)
    {
        bool enabled = (filter & flag) != 0;

        if (ImGui.Checkbox(text, ref enabled))
        {
            if (enabled)
                filter |= flag;
            else
                filter &= ~flag;
        }

        ImGui.SameLine();
    }

    private static void DrawLog(LogEntry entry)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, entry.color.PackedValue);

        if (entry.repeatCount == 1)
            ImGui.TextUnformatted($"[{entry.firstDate:HH:mm:ss}] {entry.logLevel.ToString().ToUpper()}: {entry.message}");
        else
            ImGui.TextUnformatted(
                $"[{entry.firstDate:HH:mm:ss} -> {entry.lastDate:HH:mm:ss}] {entry.logLevel.ToString().ToUpper()}: {entry.message} ×{entry.repeatCount}");

        ImGui.PopStyleColor();
    }
}
