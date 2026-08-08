// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Darkrit.Graphics;

/// <summary>
/// Simple camera that computes a matrix based on position, rotation and zoom
/// </summary>
public class Camera
{
    public Vector2 Position { get; set; } = Vector2.Zero;

    public float Rotation { get; set; } = 0f;

    public float Zoom { get; set; } = 1.0f;

    public void EditorDraw()
    {
        ImGui.Begin("Camera");
        
        ImGui.Text("X");
        ImGui.SameLine();

        float x = Position.X;
        if (ImGui.SliderFloat("##camX" + ".x", ref x, -100, 100))
        {
            Position = Position with { X = x };
        }

        ImGui.Text("Y");
        ImGui.SameLine();

        float y = Position.Y;
        if (ImGui.SliderFloat("##camY" + ".y", ref y, -100, 100))
        {
            Position = Position with { Y = y };
        }

        float rotation = float.RadiansToDegrees(Rotation);
        ImGui.SliderFloat("##rotation", ref rotation, 0f, 360.0f);
        Rotation = float.DegreesToRadians(rotation);

        float zoom = Zoom;
        ImGui.SliderFloat("##scale", ref zoom, 0.1f, 5.0f);
        Zoom = zoom;

        ImGui.End();
    }

    public Matrix GetViewMatrix(Viewport viewport)
    {
        return
            Matrix.CreateTranslation(-Position.X, Position.Y, 0) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateScale(Zoom, Zoom, 1) *
            Matrix.CreateTranslation(
                viewport.Width * 0.5f,
                viewport.Height * 0.5f,
                0);
    }

    public Vector2 ScreenToWorld(Point screenPosition, Viewport viewport)
    {
        Point viewportPosition = screenPosition - new Point(viewport.X, viewport.Y);

        Matrix view = GetViewMatrix(viewport);
        Matrix inverse = Matrix.Invert(view);

        return Vector2.Transform(viewportPosition.ToVector2(), inverse);
    }
}
