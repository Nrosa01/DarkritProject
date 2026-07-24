using Microsoft.Xna.Framework;

namespace Darkrit.Graphics.InstancedQuadRenderer;

// TODO: Add also rotation and depth
// When adding depth I need this to somehow work along spritebatcher
// I might just need to create my own or edit MonoGame spritebatcher
// so both renderers can be used in a frame respecting the depth
public struct QuadInstanceData
{
    public Vector2 Position;
    public Vector2 Size;
    public Vector4 SourceUV;
    public Color Color;
}
