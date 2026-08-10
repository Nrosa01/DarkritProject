using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Xna.Framework;

namespace Darkrit.Math;

using Math = System.Math;

public struct RectangleF : IEquatable<RectangleF>
{
    private static RectangleF emptyRectangleF;

    //
    // Summary:
    //     The x coordinate of the top-left corner of this Microsoft.Xna.Framework.RectangleF.
    [DataMember]
    public float X;

    //
    // Summary:
    //     The y coordinate of the top-left corner of this Microsoft.Xna.Framework.RectangleF.
    [DataMember]
    public float Y;

    //
    // Summary:
    //     The width of this Microsoft.Xna.Framework.RectangleF.
    [DataMember]
    public float Width;

    //
    // Summary:
    //     The height of this Microsoft.Xna.Framework.RectangleF.
    [DataMember]
    public float Height;

    //
    // Summary:
    //     Returns a Microsoft.Xna.Framework.RectangleF with X=0, Y=0, Width=0, Height=0.
    public static RectangleF Empty => emptyRectangleF;

    //
    // Summary:
    //     Returns the x coordinate of the left edge of this Microsoft.Xna.Framework.RectangleF.
    public readonly float Left => X;

    //
    // Summary:
    //     Returns the x coordinate of the right edge of this Microsoft.Xna.Framework.RectangleF.
    public readonly float Right => X + Width;

    //
    // Summary:
    //     Returns the y coordinate of the top edge of this Microsoft.Xna.Framework.RectangleF.
    public readonly float Top => Y;

    //
    // Summary:
    //     Returns the y coordinate of the bottom edge of this Microsoft.Xna.Framework.RectangleF.
    public readonly float Bottom => Y + Height;

    //
    // Summary:
    //     Whether or not this Microsoft.Xna.Framework.RectangleF has a Microsoft.Xna.Framework.RectangleF.Width
    //     and Microsoft.Xna.Framework.RectangleF.Height of 0, and a Microsoft.Xna.Framework.RectangleF.Location
    //     of (0, 0).
    public readonly bool IsEmpty
    {
        get
        {
            if (Width == 0 && Height == 0 && X == 0)
                return Y == 0;

            return false;
        }
    }

    //
    // Summary:
    //     The top-left coordinates of this Microsoft.Xna.Framework.RectangleF.
    public Vector2 Location
    {
        readonly get
        {
            return new Vector2(X, Y);
        }
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    //
    // Summary:
    //     The width-height coordinates of this Microsoft.Xna.Framework.RectangleF.
    public Vector2 Size
    {
        readonly get
        {
            return new Vector2(Width, Height);
        }
        set
        {
            Width = value.X;
            Height = value.Y;
        }
    }

    //
    // Summary:
    //     A Microsoft.Xna.Framework.Vector2 located in the center of this Microsoft.Xna.Framework.RectangleF.
    //
    //
    // Remarks:
    //     If Microsoft.Xna.Framework.RectangleF.Width or Microsoft.Xna.Framework.RectangleF.Height
    //     is an odd number, the center Vector2 will be rounded down.
    public readonly Vector2 Center => new(X + Width / 2, Y + Height / 2);

    //
    // Summary:
    //     Creates a new instance of Microsoft.Xna.Framework.RectangleF struct, with the
    //     specified position, width, and height.
    //
    // Parameters:
    //   x:
    //     The x coordinate of the top-left corner of the created Microsoft.Xna.Framework.RectangleF.
    //
    //
    //   y:
    //     The y coordinate of the top-left corner of the created Microsoft.Xna.Framework.RectangleF.
    //
    //
    //   width:
    //     The width of the created Microsoft.Xna.Framework.RectangleF.
    //
    //   height:
    //     The height of the created Microsoft.Xna.Framework.RectangleF.
    public RectangleF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    //
    // Summary:
    //     Creates a new instance of Microsoft.Xna.Framework.RectangleF struct, with the
    //     specified location and size.
    //
    // Parameters:
    //   location:
    //     The x and y coordinates of the top-left corner of the created Microsoft.Xna.Framework.RectangleF.
    //
    //
    //   size:
    //     The width and height of the created Microsoft.Xna.Framework.RectangleF.
    public RectangleF(Vector2 location, Vector2 size)
    {
        X = location.X;
        Y = location.Y;
        Width = size.X;
        Height = size.Y;
    }

    //
    // Summary:
    //     Compares whether two Microsoft.Xna.Framework.RectangleF instances are equal.
    //
    // Parameters:
    //   a:
    //     Microsoft.Xna.Framework.RectangleF instance on the left of the equal sign.
    //
    //   b:
    //     Microsoft.Xna.Framework.RectangleF instance on the right of the equal sign.
    //
    // Returns:
    //     true if the instances are equal; false otherwise.
    public static bool operator ==(RectangleF a, RectangleF b)
    {
        if (a.X == b.X && a.Y == b.Y && a.Width == b.Width)
        {
            return a.Height == b.Height;
        }

        return false;
    }

    //
    // Summary:
    //     Compares whether two Microsoft.Xna.Framework.RectangleF instances are not equal.
    //
    //
    // Parameters:
    //   a:
    //     Microsoft.Xna.Framework.RectangleF instance on the left of the not equal sign.
    //
    //
    //   b:
    //     Microsoft.Xna.Framework.RectangleF instance on the right of the not equal sign.
    //
    //
    // Returns:
    //     true if the instances are not equal; false otherwise.
    public static bool operator !=(RectangleF a, RectangleF b)
    {
        return !(a == b);
    }

    //
    // Summary:
    //     Gets whether or not the provided coordinates lie within the bounds of this Microsoft.Xna.Framework.RectangleF.
    //
    //
    // Parameters:
    //   x:
    //     The x coordinate of the Vector2 to check for containment.
    //
    //   y:
    //     The y coordinate of the Vector2 to check for containment.
    //
    // Returns:
    //     true if the provided coordinates lie inside this Microsoft.Xna.Framework.RectangleF;
    //     false otherwise.
    public readonly bool Contains(float x, float y)
    {
        if (X <= x && x < X + Width && Y <= y)
            return y < Y + Height;

        return false;
    }

    //
    // Summary:
    //     Gets whether or not the provided Microsoft.Xna.Framework.Vector2 lies within the
    //     bounds of this Microsoft.Xna.Framework.RectangleF.
    //
    // Parameters:
    //   value:
    //     The coordinates to check for inclusion in this Microsoft.Xna.Framework.RectangleF.
    //
    //
    // Returns:
    //     true if the provided Microsoft.Xna.Framework.Vector2 lies inside this Microsoft.Xna.Framework.RectangleF;
    //     false otherwise.
    public readonly bool Contains(Vector2 value)
    {
        if (X <= value.X && value.X < X + Width && Y <= value.Y)
            return value.Y < Y + Height;

        return false;
    }

    //
    // Summary:
    //     Gets whether or not the provided Microsoft.Xna.Framework.Vector2 lies within the
    //     bounds of this Microsoft.Xna.Framework.RectangleF.
    //
    // Parameters:
    //   value:
    //     The coordinates to check for inclusion in this Microsoft.Xna.Framework.RectangleF.
    //
    //
    //   result:
    //     true if the provided Microsoft.Xna.Framework.Vector2 lies inside this Microsoft.Xna.Framework.RectangleF;
    //     false otherwise. As an output parameter.
    public readonly void Contains(ref Vector2 value, out bool result) => result = X <= value.X && value.X < X + Width && Y <= value.Y && value.Y < Y + Height;

    //
    // Summary:
    //     Gets whether or not the provided Microsoft.Xna.Framework.RectangleF lies within
    //     the bounds of this Microsoft.Xna.Framework.RectangleF.
    //
    // Parameters:
    //   value:
    //     The Microsoft.Xna.Framework.RectangleF to check for inclusion in this Microsoft.Xna.Framework.RectangleF.
    //
    //
    // Returns:
    //     true if the provided Microsoft.Xna.Framework.RectangleF's bounds lie entirely
    //     inside this Microsoft.Xna.Framework.RectangleF; false otherwise.
    public readonly bool Contains(RectangleF value)
    {
        if (X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y)
            return value.Y + value.Height <= Y + Height;

        return false;
    }

    //
    // Summary:
    //     Gets whether or not the provided Microsoft.Xna.Framework.RectangleF lies within
    //     the bounds of this Microsoft.Xna.Framework.RectangleF.
    //
    // Parameters:
    //   value:
    //     The Microsoft.Xna.Framework.RectangleF to check for inclusion in this Microsoft.Xna.Framework.RectangleF.
    //
    //
    //   result:
    //     true if the provided Microsoft.Xna.Framework.RectangleF's bounds lie entirely
    //     inside this Microsoft.Xna.Framework.RectangleF; false otherwise. As an output
    //     parameter.
    public readonly void Contains(ref RectangleF value, out bool result) => result = X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y && value.Y + value.Height <= Y + Height;

    //
    // Summary:
    //     Compares whether current instance is equal to specified System.Object.
    //
    // Parameters:
    //   obj:
    //     The System.Object to compare.
    //
    // Returns:
    //     true if the instances are equal; false otherwise.
    public override readonly bool Equals(object obj)
    {
        if (obj is RectangleF rectf)
            return this == rectf;

        return false;
    }

    //
    // Summary:
    //     Compares whether current instance is equal to specified Microsoft.Xna.Framework.RectangleF.
    //
    //
    // Parameters:
    //   other:
    //     The Microsoft.Xna.Framework.RectangleF to compare.
    //
    // Returns:
    //     true if the instances are equal; false otherwise.
    public readonly bool Equals(RectangleF other) => this == other;

    //
    // Summary:
    //     Gets the hash code of this Microsoft.Xna.Framework.RectangleF.
    //
    // Returns:
    //     Hash code of this Microsoft.Xna.Framework.RectangleF.
    public override readonly int GetHashCode() => (((17 * 23 + X.GetHashCode()) * 23 + Y.GetHashCode()) * 23 + Width.GetHashCode()) * 23 + Height.GetHashCode();

    //
    // Summary:
    //     Adjusts the edges of this Microsoft.Xna.Framework.RectangleF by specified horizontal
    //     and vertical amounts.
    //
    // Parameters:
    //   horizontalAmount:
    //     Value to adjust the left and right edges.
    //
    //   verticalAmount:
    //     Value to adjust the top and bottom edges.
    public void Inflate(float horizontalAmount, float verticalAmount)
    {
        X -= horizontalAmount;
        Y -= verticalAmount;
        Width += horizontalAmount * 2;
        Height += verticalAmount * 2;
    }

    //
    // Summary:
    //     Gets whether or not the other Microsoft.Xna.Framework.RectangleF Intersects with
    //     this RectangleF.
    //
    // Parameters:
    //   value:
    //     The other RectangleF for testing.
    //
    // Returns:
    //     true if other Microsoft.Xna.Framework.RectangleF Intersects with this RectangleF;
    //     false otherwise.
    public readonly bool Intersects(RectangleF value)
    {
        if (value.Left < Right && Left < value.Right && value.Top < Bottom)
        {
            return Top < value.Bottom;
        }

        return false;
    }

    //
    // Summary:
    //     Gets whether or not the other Microsoft.Xna.Framework.RectangleF Intersects with
    //     this RectangleF.
    //
    // Parameters:
    //   value:
    //     The other RectangleF for testing.
    //
    //   result:
    //     true if other Microsoft.Xna.Framework.RectangleF Intersects with this RectangleF;
    //     false otherwise. As an output parameter.
    public readonly void Intersects(ref RectangleF value, out bool result) => result = value.Left < Right && Left < value.Right && value.Top < Bottom && Top < value.Bottom;

    //
    // Summary:
    //     Creates a new Microsoft.Xna.Framework.RectangleF that contains overlapping region
    //     of two other RectangleFs.
    //
    // Parameters:
    //   value1:
    //     The first Microsoft.Xna.Framework.RectangleF.
    //
    //   value2:
    //     The second Microsoft.Xna.Framework.RectangleF.
    //
    // Returns:
    //     Overlapping region of the two RectangleFs.
    public static RectangleF Intersect(RectangleF value1, RectangleF value2)
    {
        Intersect(ref value1, ref value2, out var result);
        return result;
    }

    //
    // Summary:
    //     Creates a new Microsoft.Xna.Framework.RectangleF that contains overlapping region
    //     of two other RectangleFs.
    //
    // Parameters:
    //   value1:
    //     The first Microsoft.Xna.Framework.RectangleF.
    //
    //   value2:
    //     The second Microsoft.Xna.Framework.RectangleF.
    //
    //   result:
    //     Overlapping region of the two RectangleFs as an output parameter.
    public static void Intersect(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
    {
        if (value1.Intersects(value2))
        {
            float num = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
            float num2 = Math.Max(value1.X, value2.X);
            float num3 = Math.Max(value1.Y, value2.Y);
            float num4 = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
            result = new RectangleF(num2, num3, num - num2, num4 - num3);
        }
        else
        {
            result = new RectangleF(0, 0, 0, 0);
        }
    }

    //
    // Summary:
    //     Changes the Microsoft.Xna.Framework.RectangleF.Location of this Microsoft.Xna.Framework.RectangleF.
    //
    //
    // Parameters:
    //   offsetX:
    //     The x coordinate to add to this Microsoft.Xna.Framework.RectangleF.
    //
    //   offsetY:
    //     The y coordinate to add to this Microsoft.Xna.Framework.RectangleF.
    public void Offset(float offsetX, float offsetY)
    {
        X += offsetX;
        Y += offsetY;
    }

    //
    // Summary:
    //     Changes the Microsoft.Xna.Framework.RectangleF.Location of this Microsoft.Xna.Framework.RectangleF.
    //
    //
    // Parameters:
    //   amount:
    //     The x and y components to add to this Microsoft.Xna.Framework.RectangleF.
    public void Offset(Vector2 amount)
    {
        X += amount.X;
        Y += amount.Y;
    }

    //
    // Summary:
    //     Returns a System.String representation of this Microsoft.Xna.Framework.RectangleF
    //     in the format: {X:[Microsoft.Xna.Framework.RectangleF.X] Y:[Microsoft.Xna.Framework.RectangleF.Y]
    //     Width:[Microsoft.Xna.Framework.RectangleF.Width] Height:[Microsoft.Xna.Framework.RectangleF.Height]}
    //
    //
    // Returns:
    //     System.String representation of this Microsoft.Xna.Framework.RectangleF.
    public override readonly string ToString() => "{X:" + X + " Y:" + Y + " Width:" + Width + " Height:" + Height + "}";

    //
    // Summary:
    //     Creates a new Microsoft.Xna.Framework.RectangleF that completely contains two
    //     other RectangleFs.
    //
    // Parameters:
    //   value1:
    //     The first Microsoft.Xna.Framework.RectangleF.
    //
    //   value2:
    //     The second Microsoft.Xna.Framework.RectangleF.
    //
    // Returns:
    //     The union of the two RectangleFs.
    public static RectangleF Union(RectangleF value1, RectangleF value2)
    {
        float num = Math.Min(value1.X, value2.X);
        float num2 = Math.Min(value1.Y, value2.Y);
        return new RectangleF(num, num2, Math.Max(value1.Right, value2.Right) - num, Math.Max(value1.Bottom, value2.Bottom) - num2);
    }

    //
    // Summary:
    //     Creates a new Microsoft.Xna.Framework.RectangleF that completely contains two
    //     other RectangleFs.
    //
    // Parameters:
    //   value1:
    //     The first Microsoft.Xna.Framework.RectangleF.
    //
    //   value2:
    //     The second Microsoft.Xna.Framework.RectangleF.
    //
    //   result:
    //     The union of the two RectangleFs as an output parameter.
    public static void Union(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
    {
        result.X = Math.Min(value1.X, value2.X);
        result.Y = Math.Min(value1.Y, value2.Y);
        result.Width = Math.Max(value1.Right, value2.Right) - result.X;
        result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
    }

    //
    // Summary:
    //     Deconstruction method for Microsoft.Xna.Framework.RectangleF.
    //
    // Parameters:
    //   x:
    //
    //   y:
    //
    //   width:
    //
    //   height:
    public readonly void Deconstruct(out float x, out float y, out float width, out float height)
    {
        x = X;
        y = Y;
        width = Width;
        height = Height;
    }
}
