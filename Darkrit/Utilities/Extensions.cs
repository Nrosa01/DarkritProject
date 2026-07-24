using Microsoft.Xna.Framework;

namespace Darkrit.Utilities
{
    public static class Extensions
    {
        extension(Vector2 vector)
        {
            public Vector2 Normalized => vector == Vector2.Zero ? vector : Vector2.Normalize(vector);

            public void NormalizeZero()
            {
                if (vector == Vector2.Zero)
                    return;

                vector.Normalize();
            }
        }
    }
}
