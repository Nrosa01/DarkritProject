// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Darkrit.Math;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Point = Microsoft.Xna.Framework.Point;
using SPoint = System.Drawing.Point;

namespace Darkrit.Utilities;

public static class Extensions
{
    extension(Vector2 vector)
    {
        public Vector2 Normalized => vector == Vector2.Zero ? vector : Vector2.Normalize(vector);

        public System.Numerics.Vector2 ToSystemVector2() => new(vector.X, vector.Y);

        public void NormalizeZero()
        {
            if (vector == Vector2.Zero)
                return;

            vector.Normalize();
        }
    }

    private static Texture2D pixel;
    extension(SpriteBatch spriteBatch)
    {
        public void Draw(RectangleF rect, Color color) => spriteBatch.Draw(rect.ToRectangle(), color);

        public void Draw(RectangleF rect, Color color, float fillOpacity) => spriteBatch.Draw(rect.ToRectangle(), color, fillOpacity);

        public void Draw(Rectangle rect, Color color)
        {
            if (pixel == null)
            {
                pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                pixel.SetData([Color.White]);
            }

            spriteBatch.Draw(pixel, destinationRectangle: rect, color: color);
        }

        public void Draw(Rectangle rect, Color stroke, float fillOpacity)
        {
            if (pixel == null)
            {
                pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                pixel.SetData([Color.White]);
            }

            var fill = new Color(stroke, fillOpacity);
            spriteBatch.DrawFill(rect, fill);
            spriteBatch.DrawStroke(rect, stroke);
        }

        public void DrawFill(Rectangle rect, Color fill)
        {
            if (pixel == null)
            {
                pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                pixel.SetData([Color.White]);
            }

            spriteBatch.Draw(pixel, destinationRectangle: rect, color: fill);
        }

        public void DrawStroke(Rectangle rect, Color stroke)
        {
            if (pixel == null)
            {
                pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                pixel.SetData([Color.White]);
            }

            var left = new Rectangle(rect.Left, rect.Top, 1, rect.Height);
            var right = new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height);
            var top = new Rectangle(rect.Left, rect.Top, rect.Width, 1);
            var bottom = new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1);

            spriteBatch.Draw(pixel, destinationRectangle: left, color: stroke);
            spriteBatch.Draw(pixel, destinationRectangle: right, color: stroke);
            spriteBatch.Draw(pixel, destinationRectangle: top, color: stroke);
            spriteBatch.Draw(pixel, destinationRectangle: bottom, color: stroke);
        }
    }

    extension(SPoint sysPoint)
    {
        public Point AsMonoGamePoint() => new(sysPoint.X, sysPoint.Y);
    }
}
