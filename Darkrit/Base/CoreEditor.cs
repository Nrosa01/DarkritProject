using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Darkrit.ImGuiUtils.Themes;
using Darkrit.Scenes;
using Darkrit.Utilities;
using ExampleMonoGame;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Darkrit.Base
{
    internal class CoreEditor
    {
        public CoreEditor(GraphicsDevice GraphicsDevice, ImGuiRenderer ImGuiRenderer)
        {
            this.GraphicsDevice = GraphicsDevice;

#if EDITOR_BUILD
            _sceneTarget = new RenderTarget2D(
                            GraphicsDevice,
                            1280,
                            720,
                            false,
                            SurfaceFormat.Color,
                            DepthFormat.Depth24);

            _sceneTextureId = ImGuiRenderer.BindTexture(_sceneTarget);

            CoreStats = new(GraphicsDevice);

            ShowEditor = true;

            this.ImGuiRenderer = ImGuiRenderer;

            // Optional: Scale text and widgets for easier readability.
            var io = ImGui.GetIO();

            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

            //io.FontGlobalScale = 1.25f;
            unsafe
            {
                io.Fonts.AddFontFromFileTTF("Content/fonts/JetBrainsMono-Regular.ttf", 20);
            }
            //io.Fonts.AddFontFromFileTTF("Content/fonts/FiraCode-Regular.ttf", 16);
            ImGui.GetStyle().ScaleAllSizes(1.25f);

            PurpleComfyTheme.SetupImGuiStyle();
#endif
        }

        readonly GraphicsDevice GraphicsDevice;
        public ImGuiRenderer ImGuiRenderer { get; init; }

        public Viewport Viewport { get; internal set; }
        public CoreStats CoreStats { get; init; }

#if EDITOR_BUILD
        private RenderTarget2D _sceneTarget;
        private ImTextureRef _sceneTextureId;

        private Point _pendingViewportSize;
        private float _resizeDelay;
        private bool _hasPendingResize;
        private const float ResizeDelaySeconds = 0.01f;
#endif
        public void ToggleShow() => ShowEditor = !ShowEditor;

        public static void UnFocus() => ImGui.SetWindowFocus();

        public bool ShowEditor { get; internal set; } = false;
        public bool ViewportFocused { get; internal set; } = false;
        public bool IsGameNotFocused => !ViewportFocused && ShowEditor;
        public bool IsGameFocused => !IsGameNotFocused;

        [Conditional("EDITOR_BUILD")]
        public void Update(GameTime gameTime) => CoreStats.Update(gameTime);

#if EDITOR_BUILD

        public void RenderWithDocking(Action renderAction, GameTime gameTime)
        {
            ImGui.DockSpaceOverViewport();

            ImGui.Begin("Viewport");

            Vector2 viewportPos = ImGui.GetCursorScreenPos();
            Vector2 viewportSize = ImGui.GetContentRegionAvail();

            Viewport = new(
                (int)viewportPos.X,
                (int)viewportPos.Y,
                (int)viewportSize.X,
                (int)viewportSize.Y);

            UpdateViewportResize(viewportSize, gameTime);

            ImGui.Image(_sceneTextureId, viewportSize.ToSystemVector2());

            ViewportFocused = ImGui.IsWindowFocused();

            ImGui.End();

            GraphicsDevice.SetRenderTarget(_sceneTarget);

            renderAction?.Invoke();

            GraphicsDevice.SetRenderTarget(null);

            CoreStats.DrawStats();
        }

        private void UpdateViewportResize(Vector2 viewportSize, GameTime gameTime)
        {
            var size = new Point(
                SMath.Max(1, (int)viewportSize.X),
                SMath.Max(1, (int)viewportSize.Y));

            if (size != _pendingViewportSize)
            {
                _pendingViewportSize = size;
                _resizeDelay = ResizeDelaySeconds;
                _hasPendingResize = true;
            }

            if (!_hasPendingResize)
                return;

            _resizeDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_resizeDelay <= 0 &&
                (_sceneTarget.Width != size.X || _sceneTarget.Height != size.Y))
            {
                ResizeSceneTarget(_pendingViewportSize);
                _hasPendingResize = false;
            }
        }

        private void ResizeSceneTarget(Point size)
        {
            ImGuiRenderer.UnbindTexture(_sceneTextureId);


            _sceneTarget.Dispose();

            _sceneTarget = new RenderTarget2D(
                GraphicsDevice,
                size.X,
                size.Y);

            _sceneTextureId = ImGuiRenderer.BindTexture(_sceneTarget);
        }

        internal void Render(GameTime gameTime, Action<GameTime, CoreEditor> renderAction)
        {
            ImGuiRenderer.BeforeLayout(gameTime);

            if (ShowEditor)
            {
                RenderWithDocking(() =>
                {
                    GraphicsDevice.Clear(Color.CornflowerBlue);
                    renderAction?.Invoke(gameTime, this);
                }, gameTime);
            }
            else
            {
                GraphicsDevice.Clear(Color.CornflowerBlue);
                renderAction?.Invoke(gameTime, this);
                Viewport = GraphicsDevice.Viewport;
            }

            ImGuiRenderer.AfterLayout();
        }
#else
        internal void Render(GameTime gameTime, Action<GameTime, CoreEditor> renderAction)
        {
           GraphicsDevice.Clear(Color.CornflowerBlue);
           renderAction?.Invoke(gameTime, this);
           Viewport = GraphicsDevice.Viewport;
        }
#endif
    }
}
