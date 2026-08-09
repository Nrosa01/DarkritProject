// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using GameProject;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Darkrit.Graphics.FirstBatcher;

public class FirstBatcher
{
    private Effect _firstShader;

    private EffectParameter _viewProjection = null!;

    private const int _initialQuads = 2048;
    private const int _initialVertices = _initialQuads * 4;
    private const int _initialIndices = _initialQuads * 6;

    private SamplerState _sampler = null!;
    private Texture2D _texture = null!;
    private Matrix _view;
    private Matrix _projection;

    private FirstVertex[] _vertices = null!;
    private uint[] _indices = null!;
    private DynamicVertexBuffer _vertexBuffer = null!;
    private IndexBuffer _indexBuffer = null!;

    private int _triangleCount = 0;
    private int _vertexCount = 0;
    private int _indexCount = 0;

    private bool _indicesChanged = false;
    private uint _fromIndex = 0;
    private uint _fromVertex = 0;
    private GraphicsDevice GraphicsDevice;

    public FirstBatcher(GraphicsDevice graphicsDevice)
    {
        GraphicsDevice = graphicsDevice;

        _firstShader = Core.Content.Load<Effect>("Shared/Sprite");
        _viewProjection = _firstShader.Parameters["view_projection"];

        _vertices = new FirstVertex[_initialVertices];
        _indices = new uint[_initialIndices];

        GenerateIndexArray();

        _vertexBuffer = new DynamicVertexBuffer(GraphicsDevice, typeof(FirstVertex), _vertices.Length, BufferUsage.WriteOnly);

        _indexBuffer = new IndexBuffer(GraphicsDevice, typeof(uint), _indices.Length, BufferUsage.WriteOnly);
        _indexBuffer.SetData(_indices);
    }

    public void Begin(Texture2D texture = null, Matrix? view = null, Matrix? projection = null, SamplerState sampler = null)
    {
        Viewport viewport = GraphicsDevice.Viewport;

        _texture = texture;
        _view = view ?? Matrix.Identity;
        _projection = projection ?? Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
        _sampler = sampler ?? SamplerState.LinearClamp;
    }

    public void End()
    {
        if (_triangleCount == 0) return;

        if (_indicesChanged)
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();

            _vertexBuffer = new DynamicVertexBuffer(GraphicsDevice, typeof(FirstVertex), _vertices.Length, BufferUsage.WriteOnly);

            GenerateIndexArray();

            _indexBuffer = new IndexBuffer(GraphicsDevice, typeof(uint), _indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(_indices);

            _indicesChanged = false;
        }

        _vertexBuffer.SetData(_vertices, 0, _vertexCount, SetDataOptions.Discard);
        GraphicsDevice.SetVertexBuffer(_vertexBuffer);

        GraphicsDevice.Indices = _indexBuffer;

        _viewProjection.SetValue(_view * _projection);
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        GraphicsDevice.DepthStencilState = DepthStencilState.None;
        GraphicsDevice.BlendState = BlendState.AlphaBlend;
        GraphicsDevice.SamplerStates[0] = _sampler;

        _firstShader.CurrentTechnique.Passes[0].Apply();

        GraphicsDevice.Textures[0] = _texture;

        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _triangleCount);

        _triangleCount = 0;
        _vertexCount = 0;
        _indexCount = 0;

        // TODO: Restore old states like rasterizer, depth stencil, blend state?
    }

    public void Draw(Vector2 xy, Color? color = null)
    {
        EnsureSizeOrDouble(ref _vertices, _vertexCount + 4);
        _indicesChanged = EnsureSizeOrDouble(ref _indices, _indexCount + 6) || _indicesChanged;

        Vector2 topLeft = xy + new Vector2(0f, 0f);
        Vector2 topRight = xy + new Vector2(_texture.Width, 0f);
        Vector2 bottomRight = xy + new Vector2(_texture.Width, _texture.Height);
        Vector2 bottomLeft = xy + new Vector2(0f, _texture.Height);

        color ??= Color.White;

        _vertices[_vertexCount + 0] = new FirstVertex(
            new Vector3(topLeft, 0f),
            new Vector2(0f, 0f),
            color.Value
        );
        _vertices[_vertexCount + 1] = new FirstVertex(
            new Vector3(topRight, 0f),
            new Vector2(1f, 0f),
            color.Value
        );
        _vertices[_vertexCount + 2] = new FirstVertex(
            new Vector3(bottomRight, 0f),
            new Vector2(1f, 1f),
            color.Value
        );
        _vertices[_vertexCount + 3] = new FirstVertex(
            new Vector3(bottomLeft, 0f),
            new Vector2(0f, 1f),
            color.Value
        );

        _triangleCount += 2;
        _vertexCount += 4;
        _indexCount += 6;
    }

    private bool EnsureSizeOrDouble<T>(ref T[] array, int neededCapacity)
    {
        if (array.Length < neededCapacity)
        {
            Array.Resize(ref array, array.Length * 2);
            return true;
        }
        return false;
    }

    private void GenerateIndexArray()
    {
        for (uint i = _fromIndex, j = _fromVertex; i < _indices.Length; i += 6, j += 4)
        {
            _indices[i + 0] = j + 0;
            _indices[i + 1] = j + 1;
            _indices[i + 2] = j + 3;
            _indices[i + 3] = j + 1;
            _indices[i + 4] = j + 2;
            _indices[i + 5] = j + 3;
        }
        _fromIndex = (uint)_indices.Length;
        _fromVertex = (uint)_vertices.Length;
    }
}
