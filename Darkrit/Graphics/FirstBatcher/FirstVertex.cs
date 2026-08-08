// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameProject;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FirstVertex(Vector3 position, Vector2 textureCoordinate, Color color) : IVertexType
{
    public Vector3 Position = position;
    public Vector2 TextureCoordinate = textureCoordinate;
    public Color Color = color;
    public static readonly VertexDeclaration VertexDeclaration;

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

    public override readonly int GetHashCode()
    {
        return System.HashCode.Combine(Position, TextureCoordinate, Color);
    }

    public override readonly string ToString()
    {
        return
            "{{Position:" + Position +
            " TextureCoordinate:" + TextureCoordinate +
            " Color:" + Color +
            "}}";
    }

    public static bool operator ==(FirstVertex left, FirstVertex right)
    {
        return
            left.Position == right.Position &&
            left.TextureCoordinate == right.TextureCoordinate &&
            left.Color == right.Color;
    }

    public static bool operator !=(FirstVertex left, FirstVertex right)
    {
        return !(left == right);
    }

    public override readonly bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj.GetType() != base.GetType())
            return false;

        return this == ((FirstVertex)obj);
    }

    static FirstVertex()
    {
        int offset = 0;
        var elements = new VertexElement[] {
            GetVertexElement(ref offset, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            GetVertexElement(ref offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            GetVertexElement(ref offset, VertexElementFormat.Color, VertexElementUsage.Color, 0),
        };
        VertexDeclaration = new VertexDeclaration(elements);
    }
    private static VertexElement GetVertexElement(ref int offset, VertexElementFormat f, VertexElementUsage u, int usageIndex)
    {
        return new(OffsetInline(ref offset, Offsets[f]), f, u, usageIndex);
    }
    private static int OffsetInline(ref int value, int offset)
    {
        int old = value;
        value += offset;
        return old;
    }
    private static readonly Dictionary<VertexElementFormat, int> Offsets = new()
    {
        [VertexElementFormat.Single] = 4,
        [VertexElementFormat.Vector2] = 8,
        [VertexElementFormat.Vector3] = 12,
        [VertexElementFormat.Vector4] = 16,
        [VertexElementFormat.Color] = 4,
        [VertexElementFormat.Byte4] = 4,
        [VertexElementFormat.Short2] = 4,
        [VertexElementFormat.Short4] = 8,
        [VertexElementFormat.NormalizedShort2] = 4,
        [VertexElementFormat.NormalizedShort4] = 8,
        [VertexElementFormat.HalfVector2] = 4,
        [VertexElementFormat.HalfVector4] = 8,
    };
}
