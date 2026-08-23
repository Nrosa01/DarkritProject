// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Ported from: https://github.com/Miisan-png/ImGizmo2D

namespace Darkrit.ImGuiUtils;

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Hexa.NET.ImGui;

/// <summary>
/// Provides 2D transformation and editing gizmos for ImGui, including translation, rotation,
/// scaling, and shape manipulation.
/// </summary>
public static class ImGizmo2D
{
    private struct Context
    {
        public ImDrawListPtr DrawList;

        public Vector2 ViewOrigin;
        public Vector2 ViewSize;
        public Vector2 CameraPosition;
        public float Zoom;

        public float HandleRadius = 6.0f;
        public float LineThickness = 2.0f;
        public float SnapGrid = 0.0f;

        public uint ColIdle = Color(100, 200, 220, 255);
        public uint ColHover = Color(255, 220, 80, 255);
        public uint ColActive = Color(255, 255, 120, 255);
        public uint ColLine = Color(100, 200, 220, 180);
        public uint ColFill = Color(100, 200, 220, 30);
        public uint ColAxisX = Color(230, 70, 70, 255);
        public uint ColAxisY = Color(70, 200, 70, 255);

        public uint ActiveId;
        public uint HoveredId;

        public string ActiveName;
        public string HoveredName;

        public Vector2 GrabOffset; // so handles don't snap-jump to cursor on click

        public Context() { }
    }

    private static Context _context = new();

    /// <summary>
    /// Creates an ImGui-compatible 32-bit color from red, green, blue, and alpha components.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Color(byte r, byte g, byte b, byte a) => (uint)(r | (g << 8) | (b << 16) | (a << 24));

    // fnv-1a hash so each handle gets a unique id even across gizmos
    private static uint HashId(string id, int handle)
    {
        const uint offset = 2166136261;
        const uint prime = 16777619;

        uint hash = offset;

        foreach (char c in id)
        {
            hash ^= c;
            hash *= prime;
        }

        hash ^= unchecked((uint)(handle * 2654435761));
        return hash;
    }

    /// <summary>
    /// Sets the draw list used to render the gizmos.
    /// </summary>
    public static void SetDrawList(ImDrawListPtr drawList) => _context.DrawList = drawList;

    /// <summary>
    /// Sets the viewport origin and size used for gizmo rendering.
    /// </summary>
    public static void SetViewRect(Vector2 origin, Vector2 size)
    {
        _context.ViewOrigin = origin;
        _context.ViewSize = size;
    }

    /// <summary>
    /// Sets the camera position and zoom used to transform world coordinates to screen coordinates.
    /// </summary>
    public static void SetViewTransform(float cameraX, float cameraY, float zoom)
    {
        _context.CameraPosition = new Vector2(cameraX, cameraY);
        _context.Zoom = zoom;
    }

    /// <summary>
    /// Sets the grid size used to snap dragged values. A value of zero or less disables snapping.
    /// </summary>
    public static void SetSnapGrid(float snap) => _context.SnapGrid = snap;

    /// <summary>
    /// Sets the radius of gizmo interaction handles.
    /// </summary>
    public static void SetHandleRadius(float radius) => _context.HandleRadius = radius;

    /// <summary>
    /// Sets the thickness of gizmo lines.
    /// </summary>
    public static void SetLineThickness(float thickness) => _context.LineThickness = thickness;

    /// <summary>
    /// Sets the idle, hover, and active handle colors.
    /// </summary>
    public static void SetColors(uint idle, uint hover, uint active)
    {
        _context.ColIdle = idle;
        _context.ColHover = hover;
        _context.ColActive = active;
        _context.ColLine = idle;
    }

    /// <summary>
    /// Sets the color used for gizmo lines.
    /// </summary>
    public static void SetLineColor(uint color) => _context.ColLine = color;

    /// <summary>
    /// Sets the fill color used by filled gizmos.
    /// </summary>
    public static void SetFillColor(uint color) => _context.ColFill = color;

    /// <summary>
    /// Sets the colors used for the X and Y axes.
    /// </summary>
    public static void SetAxisColors(uint x, uint y)
    {
        _context.ColAxisX = x;
        _context.ColAxisY = y;
    }

    /// <summary>
    /// Begins a gizmo frame, resetting interaction state and applying the viewport clipping region.
    /// </summary>
    public static void BeginFrame()
    {
        _context.HoveredId = 0;
        _context.HoveredName = null;

        _context.DrawList.PushClipRect(_context.ViewOrigin, _context.ViewOrigin + _context.ViewSize, true);

        if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            _context.ActiveId = 0;
            _context.ActiveName = null;
        }
    }

    /// <summary>
    /// Ends the gizmo frame and restores the previous clipping region.
    /// </summary>
    public static void EndFrame() => _context.DrawList.PopClipRect();

    private static Vector2 WorldToScreen(Vector2 world)
    {
        return _context.ViewOrigin +
               (world - _context.CameraPosition) * _context.Zoom +
               _context.ViewSize * 0.5f;
    }

    private static Vector2 ScreenToWorld(Vector2 screen)
    {
        return (screen - _context.ViewOrigin - _context.ViewSize * 0.5f) /
            _context.Zoom + _context.CameraPosition;
    }

    private static float Snap(float value, float grid)
    {
        if (grid <= 0.0f)
            return value;

        return MathF.Round(value / grid) * grid;
    }

    private static bool HandlePoint(string parentId, int handle, ref float x, ref float y)
    {
        uint id = HashId(parentId, handle);

        Vector2 screenPosition = WorldToScreen(new Vector2(x, y));
        Vector2 mousePosition = ImGui.GetMousePos();

        float distance = Vector2.Distance(mousePosition, screenPosition);
        bool hovered = distance <= _context.HandleRadius + 3.0f;
        bool active = _context.ActiveId == id;

        if (hovered && _context.ActiveId == 0)
        {
            _context.HoveredId = id;
            _context.HoveredName = parentId;
        }

        if (hovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && _context.ActiveId == 0)
        {
            _context.ActiveId = id;
            _context.ActiveName = parentId;

            Vector2 worldPosition = ScreenToWorld(mousePosition);
            _context.GrabOffset = worldPosition - new Vector2(x, y);
        }

        bool modified = false;

        if (active && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            Vector2 worldPosition = ScreenToWorld(mousePosition) - _context.GrabOffset;

            x = Snap(worldPosition.X, _context.SnapGrid);
            y = Snap(worldPosition.Y, _context.SnapGrid);

            modified = true;
        }

        uint color = _context.ColActive;
        if(!active)
        {
            if (_context.HoveredId == id) color = _context.ColHover;
            else color = _context.ColIdle;
        }

        _context.DrawList.AddCircleFilled(screenPosition, _context.HandleRadius, color);
        _context.DrawList.AddCircle(screenPosition, _context.HandleRadius, Color(0, 0, 0, 180), 0, 1.5f);

        return modified;
    }

    // --- Gizmos -----------------------------------------------------------------

    // like HandlePoint but locked to one axis. used by Translate and Scale.
    private static bool HandleAxis(string parentId, int handle, ref float value, float x, float y, bool isX)
    {
        uint id = HashId(parentId, handle);

        Vector2 screenPosition = WorldToScreen(new Vector2(x, y));
        Vector2 mousePosition = ImGui.GetMousePos();

        float distance = Vector2.Distance(mousePosition, screenPosition);
        bool hovered = distance <= _context.HandleRadius + 4.0f;
        bool active = _context.ActiveId == id;

        if (hovered && _context.ActiveId == 0)
        {
            _context.HoveredId = id;
            _context.HoveredName = parentId;
        }

        if (hovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && _context.ActiveId == 0)
        {
            _context.ActiveId = id;
            _context.ActiveName = parentId;

            Vector2 worldPosition = ScreenToWorld(mousePosition);

            if (isX)
                _context.GrabOffset = new Vector2(worldPosition.X - value, 0.0f);
            else
                _context.GrabOffset = new Vector2(0.0f, worldPosition.Y - value);
        }

        bool modified = false;

        if (active && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            Vector2 worldPosition = ScreenToWorld(mousePosition);

            if (isX)
                value = Snap(worldPosition.X - _context.GrabOffset.X, _context.SnapGrid);
            else
                value = Snap(worldPosition.Y - _context.GrabOffset.Y, _context.SnapGrid);

            modified = true;
        }


        uint color = _context.ColActive;
        if (!active)
        {
            if (_context.HoveredId == id)
                color = _context.ColHover;
            else if (isX)
                color = _context.ColAxisX;
            else
                color = _context.ColAxisY;
        }

        _context.DrawList.AddCircleFilled(screenPosition, _context.HandleRadius - 1.0f, color);
        _context.DrawList.AddCircle(screenPosition, _context.HandleRadius - 1.0f, Color(0, 0, 0, 180), 0, 1.5f);

        return modified;
    }

    /// <summary>
    /// Displays a translation gizmo with free and axis-constrained movement handles.
    /// </summary>
    /// <returns><c>true</c> if the position was modified; otherwise, <c>false</c>.</returns>
    public static bool Translate(string id, ref float x, ref float y)
    {
        const float axisLength = 40.0f;

        bool modified = false;

        Vector2 origin = WorldToScreen(new Vector2(x, y));
        Vector2 xTip = origin + new Vector2(axisLength, 0.0f);
        Vector2 yTip = origin + new Vector2(0.0f, axisLength);

        _context.DrawList.AddLine(origin, xTip, _context.ColAxisX, _context.LineThickness);
        _context.DrawList.AddLine(origin, yTip, _context.ColAxisY, _context.LineThickness);

        _context.DrawList.AddTriangleFilled(
            new Vector2(xTip.X + 6.0f, xTip.Y),
            new Vector2(xTip.X - 2.0f, xTip.Y - 5.0f),
            new Vector2(xTip.X - 2.0f, xTip.Y + 5.0f),
            _context.ColAxisX);

        _context.DrawList.AddTriangleFilled(
            new Vector2(yTip.X, yTip.Y + 6.0f),
            new Vector2(yTip.X - 5.0f, yTip.Y - 2.0f),
            new Vector2(yTip.X + 5.0f, yTip.Y - 2.0f),
            _context.ColAxisY);

        float xHandle = x + axisLength / _context.Zoom;
        float yHandle = y + axisLength / _context.Zoom;

        if (HandleAxis(id, 1, ref x, xHandle, y, true))
            modified = true;

        if (HandleAxis(id, 2, ref y, x, yHandle, false))
            modified = true;

        if (HandlePoint(id, 0, ref x, ref y))
            modified = true;

        return modified;
    }

    /// <summary>
    /// Displays a rectangular gizmo with draggable corner handles for resizing.
    /// </summary>
    /// <returns><c>true</c> if the rectangle was modified; otherwise, <c>false</c>.</returns>
    public static bool Rect(string id, ref float x, ref float y, ref float width, ref float height)
    {
        bool modified = false;

        Vector2 topLeft = WorldToScreen(new Vector2(x, y));
        Vector2 bottomRight = WorldToScreen(new Vector2(x + width, y + height));

        _context.DrawList.AddRectFilled(topLeft, bottomRight, _context.ColFill);

        _context.DrawList.AddRect(
            topLeft,
            bottomRight,
            _context.ColLine,
            0.0f,
            ImDrawFlags.None,
            _context.LineThickness);

        Vector2[] corners =
        [
            new(x, y),
            new(x + width, y),
            new(x, y + height),
            new(x + width, y + height)
        ];

        for (int i = 0; i < corners.Length; i++)
        {
            Vector2 corner = corners[i];

            if (!HandlePoint(id, i + 1, ref corner.X, ref corner.Y))
                continue;

            modified = true;

            switch (i)
            {
                case 0:
                    width += x - corner.X;
                    height += y - corner.Y;
                    x = corner.X;
                    y = corner.Y;
                    break;

                case 1:
                    width = corner.X - x;
                    height += y - corner.Y;
                    y = corner.Y;
                    break;

                case 2:
                    width += x - corner.X;
                    x = corner.X;
                    height = corner.Y - y;
                    break;

                case 3:
                    width = corner.X - x;
                    height = corner.Y - y;
                    break;
            }
        }

        width = MathF.Max(1.0f, width);
        height = MathF.Max(1.0f, height);

        return modified;
    }

    /// <summary>
    /// Displays a circular gizmo with draggable center and radius handles.
    /// </summary>
    /// <returns><c>true</c> if the circle was modified; otherwise, <c>false</c>.</returns>
    public static bool Circle(string id, ref float centerX, ref float centerY, ref float radius)
    {
        bool modified = false;

        Vector2 center = WorldToScreen(new Vector2(centerX, centerY));
        float screenRadius = radius * _context.Zoom;

        _context.DrawList.AddCircleFilled(center, screenRadius, _context.ColFill);
        _context.DrawList.AddCircle(center, screenRadius, _context.ColLine, 0, _context.LineThickness);

        modified |= HandlePoint(id, 0, ref centerX, ref centerY);

        float edgeX = centerX + radius;
        float edgeY = centerY;

        if (HandlePoint(id, 1, ref edgeX, ref edgeY))
        {
            float dx = edgeX - centerX;
            float dy = edgeY - centerY;

            radius = MathF.Max(1.0f, MathF.Sqrt(dx * dx + dy * dy));
            modified = true;
        }

        return modified;
    }

    /// <summary>
    /// Displays a rotation gizmo around the specified center.
    /// </summary>
    /// <returns><c>true</c> if the angle was modified; otherwise, <c>false</c>.</returns>
    public static bool Rotate(string id, float x, float y, ref float angle)
    {
        const float ringRadius = 50.0f;

        Vector2 center = WorldToScreen(new Vector2(x, y));
        float radians = MathF.PI / 180.0f * angle;

        Vector2 handlePosition = center + new Vector2(
            MathF.Cos(radians),
            MathF.Sin(radians)) * ringRadius;

        _context.DrawList.AddCircle(center, ringRadius, _context.ColLine, 0, _context.LineThickness);

        _context.DrawList.AddLine(
            center,
            handlePosition,
            _context.ColActive,
            _context.LineThickness + 1.0f);

        uint handleId = HashId(id, 1);
        Vector2 mouse = ImGui.GetMousePos();

        bool hovered = Vector2.Distance(mouse, handlePosition) <= _context.HandleRadius + 3.0f;
        bool active = _context.ActiveId == handleId;

        if (hovered && _context.ActiveId == 0)
        {
            _context.HoveredId = handleId;
            _context.HoveredName = id;
        }

        if (hovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && _context.ActiveId == 0)
        {
            _context.ActiveId = handleId;
            _context.ActiveName = id;
        }

        bool modified = false;

        if (active && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            Vector2 direction = mouse - center;

            float newAngle = MathF.Atan2(direction.Y, direction.X) *
                             180.0f / MathF.PI;

            if (_context.SnapGrid > 0.0f)
                newAngle = Snap(newAngle, _context.SnapGrid);

            angle = newAngle;
            modified = true;
        }

        uint color = _context.ColActive;

        if (!active)
        {
            if (hovered)
                color = _context.ColHover;
            else
                color = _context.ColIdle;
        }

        _context.DrawList.AddCircleFilled(handlePosition, _context.HandleRadius, color);

        _context.DrawList.AddCircle(
            handlePosition,
            _context.HandleRadius,
            Color(0, 0, 0, 180),
            0,
            1.5f);

        return modified;
    }

    /// <summary>
    /// Displays a scaling gizmo with independent X and Y axis handles.
    /// </summary>
    /// <returns><c>true</c> if the scale was modified; otherwise, <c>false</c>.</returns>
    public static bool Scale(string id, float x, float y, ref float scaleX, ref float scaleY)
    {
        const float axisLength = 50.0f;
        const float boxSize = 5.0f;

        bool modified = false;

        Vector2 origin = WorldToScreen(new Vector2(x, y));

        Vector2 xEnd = origin + new Vector2(axisLength * scaleX, 0.0f);
        Vector2 yEnd = origin + new Vector2(0.0f, axisLength * scaleY);

        _context.DrawList.AddLine(origin, xEnd, _context.ColAxisX, _context.LineThickness);
        _context.DrawList.AddLine(origin, yEnd, _context.ColAxisY, _context.LineThickness);

        _context.DrawList.AddRectFilled(
            xEnd - new Vector2(boxSize),
            xEnd + new Vector2(boxSize),
            _context.ColAxisX);

        _context.DrawList.AddRectFilled(
            yEnd - new Vector2(boxSize),
            yEnd + new Vector2(boxSize),
            _context.ColAxisY);

        Vector2 mouse = ImGui.GetMousePos();

        uint xId = HashId(id, 1);
        uint yId = HashId(id, 2);

        bool hoveredX = Vector2.Distance(mouse, xEnd) <= _context.HandleRadius + 4.0f;

        if (hoveredX && _context.ActiveId == 0)
        {
            _context.HoveredId = xId;
            _context.HoveredName = id;
        }

        if (hoveredX && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && _context.ActiveId == 0)
        {
            _context.ActiveId = xId;
            _context.ActiveName = id;
            _context.GrabOffset = new Vector2(mouse.X, 0.0f);
        }

        bool hoveredY = Vector2.Distance(mouse, yEnd) <= _context.HandleRadius + 4.0f;

        if (hoveredY && _context.ActiveId == 0)
        {
            _context.HoveredId = yId;
            _context.HoveredName = id;
        }

        else if (hoveredY && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && _context.ActiveId == 0)
        {
            _context.ActiveId = yId;
            _context.ActiveName = id;
            _context.GrabOffset = new Vector2(0.0f, mouse.Y);
        }

        bool activeX = _context.ActiveId == xId;
        bool activeY = _context.ActiveId == yId;

        if (activeX && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            scaleX += (mouse.X - _context.GrabOffset.X) / axisLength;
            scaleX = MathF.Max(0.1f, scaleX);
            _context.GrabOffset = new Vector2(mouse.X, 0.0f);
            modified = true;
        }
        else if (activeY && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            scaleY += (mouse.Y - _context.GrabOffset.Y) / axisLength;
            scaleY = MathF.Max(0.1f, scaleY);
            _context.GrabOffset = new Vector2(0.0f, mouse.Y);
            modified = true;
        }

        if (activeX)
            _context.DrawList.AddRectFilled(xEnd - new Vector2(boxSize), xEnd + new Vector2(boxSize), _context.ColActive);
        else if (hoveredX)
            _context.DrawList.AddRectFilled(xEnd - new Vector2(boxSize), xEnd + new Vector2(boxSize), _context.ColHover);

        if (activeY)
            _context.DrawList.AddRectFilled(yEnd - new Vector2(boxSize), yEnd + new Vector2(boxSize), _context.ColActive);
        else if (hoveredY)
            _context.DrawList.AddRectFilled(yEnd - new Vector2(boxSize), yEnd + new Vector2(boxSize), _context.ColHover);

        modified |= HandlePoint(id, 0, ref x, ref y);

        return modified;
    }

    /// <summary>
    /// Displays a draggable point gizmo.
    /// </summary>
    /// <returns><c>true</c> if the point was modified; otherwise, <c>false</c>.</returns>
    public static bool Point(string id, ref float x, ref float y) => HandlePoint(id, 0, ref x, ref y);

    /// <summary>
    /// Displays a line with draggable handles at both endpoints.
    /// </summary>
    /// <returns><c>true</c> if either endpoint was modified; otherwise, <c>false</c>.</returns>
    public static bool Line(string id, ref float x1, ref float y1, ref float x2, ref float y2)
    {
        Vector2 start = WorldToScreen(new Vector2(x1, y1));
        Vector2 end = WorldToScreen(new Vector2(x2, y2));

        _context.DrawList.AddLine(
            start,
            end,
            _context.ColLine,
            _context.LineThickness);

        bool modified = false;

        if (HandlePoint(id, 0, ref x1, ref y1))
            modified = true;

        if (HandlePoint(id, 1, ref x2, ref y2))
            modified = true;

        return modified;
    }

    /// <summary>
    /// Displays a polygon with draggable handles for each vertex.
    /// </summary>
    /// <returns><c>true</c> if any vertex was modified; otherwise, <c>false</c>.</returns>
    public static bool Polygon(string id, Span<Vector2> points)
    {
        if (points.Length < 2)
            return false;

        bool modified = false;

        for (int i = 0; i < points.Length; i++)
        {
            int next = (i + 1) % points.Length;

            Vector2 a = WorldToScreen(points[i]);
            Vector2 b = WorldToScreen(points[next]);

            _context.DrawList.AddLine(a, b, _context.ColLine, _context.LineThickness);
        }

        for (int i = 0; i < points.Length; i++)
            modified |= HandlePoint(id, i, ref points[i].X, ref points[i].Y);

        return modified;
    }

    /// <summary>
    /// Gets whether any gizmo handle is currently hovered by the mouse.
    /// </summary>
    /// <returns><c>true</c> if a handle is hovered; otherwise, <c>false</c>.</returns>
    public static bool IsHovered() => _context.HoveredId != 0;

    /// <summary>
    /// Gets whether any gizmo handle is currently being dragged.
    /// </summary>
    /// <returns><c>true</c> if a handle is active; otherwise, <c>false</c>.</returns>
    public static bool IsActive() => _context.ActiveId != 0;

    /// <summary>
    /// Gets the identifier of the currently active gizmo, if any.
    /// </summary>
    /// <returns>The active gizmo identifier, or <c>null</c> if no gizmo is active.</returns>
    public static string GetActiveId() => _context.ActiveName;

    /// <summary>
    /// Gets the identifier of the currently hovered gizmo, if any.
    /// </summary>
    /// <returns>The hovered gizmo identifier, or <c>null</c> if no gizmo is hovered.</returns>
    public static string GetHoveredId() => _context.HoveredName;
}