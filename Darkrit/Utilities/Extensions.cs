using Microsoft.Xna.Framework;
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

    extension(SPoint sysPoint)
    {
        public Point AsMonoGamePoint() => new(sysPoint.X, sysPoint.Y);
    }
}
