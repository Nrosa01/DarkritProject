using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Darkrit.Graphics.InstancedQuadRenderer;

// TODO: Write the todo tasks in HackNpLan
// TODO: Add rotation
// TODO: Add scale
// TODO: Add depth 
// TODO: Add SamplerState
// TODO: Add BlendState
// TODO: Add tests if possible, I need to check MonoGame
//       repo to see if they do SpriteBatch tests
// TODO: Create a new renderer that uses indirect drawing to
//       use a effect with different parameters in the same drawcall

public class InstancedQuadRenderer
{
    private readonly GraphicsDevice _graphicsDevice;

    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;

    private readonly DynamicVertexBuffer _instanceBuffer;
    private readonly QuadInstanceData[] _instanceData = new QuadInstanceData[250_000];
    private int _instanceCount;

    // Default effect. Right now I assume this base effect is used
    // I need to check how to support custom effectss
    private readonly Effect _effect;

    // Texture used for drawing, right now I only support a 
    // single texture between Begin/End. I might keep this renderer
    // for particles and derive a class to support multiple textures
    private Texture2D _texture;

    private bool _beginCalled;
    private int _primitiveCount;

    public InstancedQuadRenderer(GraphicsDevice graphicsDevice, ContentManager content)
    {
        _graphicsDevice = graphicsDevice;
        _effect = content.Load<Effect>("shared/InstanceQuadEffect"); // TODO: Create custom ContenMananger for the library

        CreateQuad();

        VertexDeclaration instanceVertexDeclaration = new(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 1),           // Destination Position
            new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),  // Destination Size
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2), // Source UV
            new VertexElement(32, VertexElementFormat.Color, VertexElementUsage.Color, 1)                // Tint
        );

        _instanceBuffer = new DynamicVertexBuffer(
            _graphicsDevice,
            instanceVertexDeclaration,
            _instanceData.Length,
            BufferUsage.WriteOnly);
    }

    private void CreateQuad()
    {
        VertexPositionTexture[] vertices =
        {
            new(new Vector3(0, 0, 0), new Vector2(0, 0)),
            new(new Vector3(1, 0, 0), new Vector2(1, 0)),
            new(new Vector3(1, 1, 0), new Vector2(1, 1)),
            new(new Vector3(0, 1, 0), new Vector2(0, 1)),
        };

        short[] indices =
        {
            0, 1, 2,
            0, 2, 3
        };

        _primitiveCount = 2; // Two triangles quad

        _vertexBuffer = new VertexBuffer(
            _graphicsDevice,
            VertexPositionTexture.VertexDeclaration,
            vertices.Length,
            BufferUsage.WriteOnly);

        _vertexBuffer.SetData(vertices);

        _indexBuffer = new IndexBuffer(
            _graphicsDevice,
            IndexElementSize.SixteenBits,
            indices.Length,
            BufferUsage.WriteOnly);

        _indexBuffer.SetData(indices);
    }

    public void Begin(Matrix? transformMatrix = null)
    {
        if (_beginCalled)
            throw new InvalidOperationException("Begin() called twice.");

        _instanceCount = 0;
        _texture = null;

        Matrix transform = transformMatrix ??
            Matrix.CreateOrthographicOffCenter(
                0,
                _graphicsDevice.Viewport.Width,
                _graphicsDevice.Viewport.Height,
                0,
                0,
                1);

        _effect.Parameters["Transform"].SetValue(transform);

        _beginCalled = true;
    }

    private void Flush()
    {
        if (_instanceCount == 0)
            return;

        // Update the instance buffer with the data:
        _instanceBuffer.SetData(
            _instanceData,
            0,
            _instanceCount,
            // Tells the driver the previous contents are no longer needed.
            // This avoids CPU stalls if the GPU is still using the old buffer.
            SetDataOptions.Discard);

        _effect.Parameters["Texture"].SetValue(_texture);

        // Bind the buffers to the GraphicsDevice, first the mesh, next the instance data.
        _graphicsDevice.SetVertexBuffers(
            new VertexBufferBinding(_vertexBuffer, 0, 0),
            new VertexBufferBinding(_instanceBuffer, 0, 1));

        //Tell the GraphicsDevice what indices describe the triangles in the vertexbuffer.
        _graphicsDevice.Indices = _indexBuffer;

        foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            _graphicsDevice.DrawInstancedPrimitives(
                PrimitiveType.TriangleList,
                0, // baseVertex, we begin at the first (zero-based) vertex.
                0, // startIndex, we begin also at the first (zero-based) datapoint.
                _primitiveCount, // Primitive count (two triangles)
                _instanceCount);
        }

        _instanceCount = 0;
    }

    public void Draw(
        Texture2D texture,
        Rectangle destinationRectangle,
        Rectangle? sourceRectangle,
        Color color)
    {
        if (!_beginCalled)
            throw new InvalidOperationException("Begin() must be called first.");

        if (_instanceCount >= _instanceData.Length)
            Flush();

        _texture ??= texture;

        if (_texture != texture)
            throw new InvalidOperationException("This renderer currently supports a single texture per Begin/End.");

        Vector4 sourceUv;

        if (sourceRectangle.HasValue)
        {
            Rectangle src = sourceRectangle.Value;

            // TODO: Remove division AND profile to see if it makes a difference.
            // MonoGame SpriteBatch uses texture.TexelWidth
            // but it's internal, so idk, I guess I'll have to fork
            // MonoGame and make it public, I want that field
            // https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/Graphics/Texture2D.cs
            sourceUv = new Vector4(
                src.X / (float)texture.Width,
                src.Y / (float)texture.Height,
                src.Width / (float)texture.Width,
                src.Height / (float)texture.Height);
        }
        else
        {
            sourceUv = new Vector4(0, 0, 1, 1);
        }

        ref QuadInstanceData instance = ref _instanceData[_instanceCount++];

        instance.Position = new Vector2(destinationRectangle.X, destinationRectangle.Y);
        instance.Size = new Vector2(destinationRectangle.Width, destinationRectangle.Height);
        instance.SourceUV = sourceUv;
        instance.Color = color;
    }

    public void End()
    {
        if (!_beginCalled)
            throw new InvalidOperationException("Begin() must be called first.");

        Flush();

        _beginCalled = false;
    }
}