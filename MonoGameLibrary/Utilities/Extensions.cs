using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoGameLibrary.Utilities
{
    public static class Extensions
    {
        extension(Vector2 vector){
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
