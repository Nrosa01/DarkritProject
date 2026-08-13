using Darkrit.Math;

namespace Darkrit.Physics.Boxy2D;

public struct Body<T>
{
    public RectangleF Bounds;
    public uint Layer;
    public uint Mask;
    public T UserData;
}
