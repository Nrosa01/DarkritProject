using System;
using System.Diagnostics;
using Darkrit.ImGuiUtils.Themes;
using Darkrit.Utilities;
using ExampleMonoGame;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Darkrit.Editor;

/// <summary>
/// Groups almost all the editor functionality that won't be shipped or run in the game
/// Only thing it owns relevant to the publish build is the Viewport, and even that could be
/// abstracted but I didn't had the need
/// </summary>
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

        EditorOverlayInstance = new(Core.InputRecorder);
#else
        Viewport = GraphicsDevice.Viewport;
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
    
    private readonly EditorOverlay EditorOverlayInstance;
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

    /// <summary>
    /// Renders the game to a render texture that than is renderer to an ImGui Image
    /// that acts as a viewport
    /// </summary>
    /// <param name="renderAction">The render action that will be drawn to the render texture</param>
    /// <param name="gameTime"></param>
    public void RenderWithDocking(Scenes.Scene scene, GameTime gameTime)
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

        scene?.Draw(gameTime);
        scene?.EditorDraw(gameTime);
        EditorOverlayInstance.Draw(gameTime);

        GraphicsDevice.SetRenderTarget(null);

        CoreStats.DrawStats();
    }

    /// <summary>
    /// Checks if viewport needs to be resized and resizes it <see cref="ResizeDelaySeconds"/> seconds
    /// passed since the last time it was resized. If the <paramref name="viewportSize"/> is the same
    /// as before no resize will be attempted.
    /// in <see cref="ResizeDelaySeconds"/> seconds
    /// </summary>
    /// <param name="viewportSize">Requested new viewport size</param>
    /// <param name="gameTime"></param>
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

    /// <summary>
    /// Disposes of the previous render target, creates a new one and binds it to the ImGui image
    /// </summary>
    /// <param name="size">New size of the render target</param>
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

    /// <summary>
    /// Renders a scene. Depending on <see cref="ShowEditor"/> it will draw it to a render texture
    /// and then show the editor UI or just draw the scene without editor UI.
    /// ÍmGui can be called at any time during the render action
    /// </summary>
    /// <param name="gameTime"></param>
    /// <param name="renderAction">The render function to draw</param>
    internal void Render(GameTime gameTime, Scenes.Scene scene)
    {
        CoreStats.ProfileStartRender();

        ImGuiRenderer.BeforeLayout(gameTime);

        if (ShowEditor)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            RenderWithDocking(scene, gameTime);
        }
        else
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            scene?.Draw(gameTime);
            Viewport = GraphicsDevice.Viewport;
        }

        ImGuiRenderer.AfterLayout();

        CoreStats.ProfileEndRender(gameTime);
    }
#else
    /// <summary>
    /// Renders a scene
    /// It is not possible to call ImGui during this render
    /// </summary>
    /// <param name="gameTime"></param>
    /// <param name="renderAction">The render function to draw</param>
    internal void Render(GameTime gameTime, Action<GameTime, CoreEditor> renderAction)
    {
       GraphicsDevice.Clear(Color.CornflowerBlue);
       scene?.Draw(gameTime);
    }
#endif
}
