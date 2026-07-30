using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Darkrit.Graphics
{
    public class Camera
    {
        public Vector2 Position { get; set; } = Vector2.Zero;

        public float Rotation { get; set; } = 0f;

        public Vector2 Zoom { get; set; } = Vector2.One;

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

            float zoom = Zoom.X;
            ImGui.SliderFloat("##scale", ref zoom, 0.1f, 5.0f);
            Zoom = Vector2.One * zoom;

            ImGui.End();
        }

        public Matrix GetViewMatrix(Viewport viewport)
        {
            return
                Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
                Matrix.CreateRotationZ(-Rotation) *
                Matrix.CreateScale(Zoom.X, Zoom.Y, 1) *
                Matrix.CreateTranslation(
                    viewport.Width * 0.5f,
                    viewport.Height * 0.5f,
                    0);
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition, Viewport viewport)
        {
            Vector2 viewportPosition = screenPosition - new Vector2(viewport.X, viewport.Y);

            Matrix view = GetViewMatrix(viewport);
            Matrix inverse = Matrix.Invert(view);

            return Vector2.Transform(viewportPosition, inverse);
        }
    }
}
