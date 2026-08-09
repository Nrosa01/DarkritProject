#nullable enable

using System;
using Hexa.NET.ImGui;

namespace Darkrit.Editor;

public static class EditorModals
{
    private static Action? _drawModal;
    private static string? _modalId;

    private static bool _openRequested;

    /// <summary>
    /// Queues a modal to open for the next frame. It is important that the <paramref name="draw"/> action
    /// uses <see cref="Close"/> to close the modal, otherwise it will be opened forever.
    /// </summary>
    /// <param name="id">String id of the modal. Must be unique between modals</param>
    /// <param name="draw">Draw function for the modal</param>
    public static void Open(string id, Action draw)
    {
        _modalId = id;
        _drawModal = draw;
        _openRequested = true;
    }

    /// <summary>
    /// Draws the queued modal, if any
    /// </summary>
    public static void Draw()
    {
        if (_drawModal == null || _modalId == null)
            return;

        if (_openRequested)
        {
            ImGui.OpenPopup(_modalId);
            _openRequested = false;
        }

        if (ImGui.BeginPopupModal(_modalId, ImGuiWindowFlags.AlwaysAutoResize))
        {
            _drawModal();

            ImGui.EndPopup();
        }
        else if (!ImGui.IsPopupOpen(_modalId))
        {
            _drawModal = null;
            _modalId = null;
        }
    }

    /// <summary>
    /// Closes the current modal
    /// </summary>
    public static void Close()
    {
        ImGui.CloseCurrentPopup();

        _drawModal = null;
        _modalId = null;
        _openRequested = false;
    }
}