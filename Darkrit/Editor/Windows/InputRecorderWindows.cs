using System;
using System.Collections.Generic;
using System.Text;
using Darkrit.ImGuiUtils;
using Darkrit.InputSystem;
using Darkrit.InputSystem.Providers;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;

namespace Darkrit.Editor.Windows
{
    internal class InputRecorderWindow(InputRecordingController recording)
    {
        public void Draw(GameTime _)
        {
            ImGui.Begin("Input Replay");

            if (ImGuiEx.DisableButton(
                "Record",
                recording.IsRecording || recording.RecordingRequested))
            {
                recording.RequestRecording();
            }

            if (recording.RecordingRequested && ImGui.Button("Stop recording quest"))
                recording.CancelRecording();

            if (ImGuiEx.DisableButton(
                "Stop recording",
                !recording.IsRecording))
            {
                recording.StopRecording();
            }

            if (ImGuiEx.DisableButton(
                "Replay saved Input",
                !recording.HasRecording))
            {
                recording.StartReplay();
            }

            if (recording.IsRecording)
                ImGui.Text($"Recording Frame {recording.RecordedFrames}");

            if (recording.IsReplaying)
                ImGui.Text(
                    $"Replaying frame {recording.CurrentFrame} or {recording.TotalFrames}");

            ImGui.End();
        }
    }
}
