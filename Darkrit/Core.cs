using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ImGuiNET;
using ImGuiNET.SampleProgram.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Darkrit.Content;
using Darkrit.Graphics;
using Darkrit.InputSystem;
using Darkrit.Scenes;
using Darkrit.Utilities;
using TinyFmod;

namespace Darkrit;
public class Core : Game
{

    #region Stats
    private const int HistorySize = 240;
    private readonly float[] _cpuRenderHistory = new float[HistorySize];
    private readonly float[] _cpuProcessHistory = new float[HistorySize];
    private int _historyIndex;

    public void AddCpuRenderTime(float ms)
    {
        _cpuRenderHistory[_historyIndex] = ms;
        _historyIndex = (_historyIndex + 1) % HistorySize;
    }

    private readonly Stopwatch _frameTimer = new();
    private readonly Stopwatch _processTimer = new();

    private float _cpuRenderMs;
    private float _cpuRenderAverageMs;

    private float _cpuProcessMs;
    private float _cpuProcessAverageMs;
    
    private float _fps;

    private readonly float[] _fpsHistory = new float[HistorySize];
    private int _fpsHistoryIndex;

    public void DrawStats()
    {
        ImGui.Begin("Renderer Stats");

        ImGui.Text($"FPS              : {_fps:0.0}");
        ImGui.Text($"CPU Compute Time : {_cpuProcessAverageMs:0.00} ms");
        ImGui.Text($"CPU Render Time  : {_cpuRenderAverageMs:0.00} ms");
        ImGui.Text($"Draw Calls       : {GraphicsDevice.Metrics.DrawCount}");
        ImGui.Text($"Sprites          : {GraphicsDevice.Metrics.SpriteCount}");
        ImGui.Text($"Primitives       : {GraphicsDevice.Metrics.PrimitiveCount}");
        ImGui.Text($"Textures         : {GraphicsDevice.Metrics.TextureCount}");
        ImGui.Text($"Targets          : {GraphicsDevice.Metrics.TargetCount}");
        ImGui.Text($"Clears           : {GraphicsDevice.Metrics.ClearCount}");

        ImGui.Separator();

        ImGui.PlotLines(
            "FPS",
            ref _fpsHistory[0],
            _fpsHistory.Length,
            _fpsHistoryIndex,
            $"{_fps:0.00}",
            0,
            60,
            new Vector2(0, 60).ToNumerics());

        ImGui.PlotLines(
            "CPU Process (ms)",
            ref _cpuProcessHistory[0],
            _cpuProcessHistory.Length,
            _historyIndex,
            $"{_cpuProcessMs:0.00} ms",
            0,
            20,
            new Vector2(0, 60).ToNumerics());

        ImGui.PlotLines(
            "CPU Frame (ms)",
            ref _cpuRenderHistory[0],
            _cpuRenderHistory.Length,
            _historyIndex,
            $"{_cpuRenderMs:0.00} ms",
            0,
            20,
            new Vector2(0, 60).ToNumerics());

        ImGui.End();
    }
    #endregion

    internal static Core s_instance;

    /// <summary>
    /// Gets a reference to the Core instance.
    /// </summary>
    public static Core Instance => s_instance;

    // The scene that is currently active.
    private static Scene s_activeScene;

    // The next scene to switch to, if there is one.++++++
    private static Scene s_nextScene;

    /// <summary>
    /// Gets the graphics device manager to control the presentation of graphics.
    /// </summary>
    public static GraphicsDeviceManager Graphics { get; private set; }

    /// <summary>
    /// Gets the graphics device used to create graphical resources and perform primitive rendering.
    /// </summary>
    public static new GraphicsDevice GraphicsDevice { get; private set; }

    /// <summary>
    /// Gets the sprite batch used for all 2D rendering.
    /// </summary>
    public static SpriteBatch SpriteBatch { get; private set; }

    /// <summary>
    /// Gets the content manager used to load global assets.
    /// </summary>
    public static new ContentManager Content { get; private set; }

    /// <summary>
    /// Gets a reference to the input management system.
    /// </summary>
    public static Darkrit.InputSystem.Input Input { get; private set; }

    public static FmodStudio FMOD { get; private set; }

    /// <summary>  
    /// Gets the ImGui renderer used for debug UIs.  
    /// </summary>  
    public static ImGuiRenderer ImGuiRenderer { get; private set; }

    /// <summary>  
    /// Gets a runtime generated 1x1 pixel texture.  
    /// </summary>  
    public static Texture2D Pixel { get; private set; }

    /// <summary>
    /// Gets or Sets a value that indicates if the game should exit when the esc key on the keyboard is pressed.
    /// </summary>
    public static bool ExitOnEscape { get; set; }

    /// <summary>
    /// Creates a new Core instance.
    /// </summary>
    /// <param name="title">The title to display in the title bar of the game window.</param>
    /// <param name="width">The initial width, in pixels, of the game window.</param>
    /// <param name="height">The initial height, in pixels, of the game window.</param>
    /// <param name="fullScreen">Indicates if the game should start in fullscreen mode.</param>
    public Core(string title, int width, int height, bool fullScreen)
    {
        //ContentManagerExtensions.StartContentWatcherTask();

        // Ensure that multiple cores are not created.
        if (s_instance != null)
            throw new InvalidOperationException($"Only a single Core instance can be created");

        // Store reference to engine for global member access.
        s_instance = this;

        // Create a new graphics device manager.
        Graphics = new(this)
        {
            // Set the graphics defaults.
            PreferredBackBufferWidth = width,
            PreferredBackBufferHeight = height,
            IsFullScreen = fullScreen
        };

        // Apply the graphic presentation changes.
        Graphics.ApplyChanges();

        // Set the window title.
        Window.Title = title;

        // Set the core's content manager to a reference of the base Game's
        // content manager.
        Content = base.Content;

        // Set the root directory for content.
        Content.RootDirectory = "Content";

        // Mouse is visible by default.
        IsMouseVisible = true;

        // Create a new input manager.
        Input = new Darkrit.InputSystem.Input();
    }

    protected override void Update(GameTime gameTime)
    {
        FMOD.Update();

        // Update the input manager.
        Input.Update(gameTime);

        if (ExitOnEscape && Input.WasKeyJustPressed(Keys.Escape))
            Exit();

        // if there is a next scene waiting to be switch to, then transition
        // to that scene.
        if (s_nextScene != null)
            TransitionScene();

        Content.ReloadChangedAssets();

        _processTimer.Restart();
        
        // If there is an active scene, update it.
        s_activeScene?.Update(gameTime);

        _processTimer.Stop();

        _cpuProcessMs = (float)_processTimer.Elapsed.TotalMilliseconds;
        _fps = (float)(1.0 / gameTime.ElapsedGameTime.TotalSeconds);

        _cpuProcessHistory[_historyIndex] = _cpuProcessMs;
        _historyIndex = (_historyIndex + 1) % HistorySize;

        const float alpha = 0.05f;
        _cpuProcessAverageMs += (_cpuProcessMs - _cpuProcessAverageMs) * alpha;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _frameTimer.Restart();
        // If there is an active scene, draw it.
        s_activeScene?.Draw(gameTime);

        _frameTimer.Stop();

        _cpuRenderMs = (float)_frameTimer.Elapsed.TotalMilliseconds;
        _fps = (float)(1.0 / gameTime.ElapsedGameTime.TotalSeconds);

        _cpuRenderHistory[_historyIndex] = _cpuRenderMs;
        _historyIndex = (_historyIndex + 1) % HistorySize;

        _fpsHistory[_fpsHistoryIndex] = _fps;
        _fpsHistoryIndex = (_fpsHistoryIndex + 1) % HistorySize;

        const float alpha = 0.05f;
        _cpuRenderAverageMs += (_cpuRenderMs - _cpuRenderAverageMs) * alpha;

        Core.ImGuiRenderer.BeforeLayout(gameTime);
        s_activeScene?.DebugDraw(gameTime);
        Material.DrawVisibleDebugUi();
        DrawStats();
        Core.ImGuiRenderer.AfterLayout();

        base.Draw(gameTime);
    }

    public static void ChangeScene(Scene next)
    {
        // Only set the next scene value if it is not the same
        // instance as the currently active scene.
        if (s_activeScene != next)
            s_nextScene = next;
    }

    private static void TransitionScene()
    {
        // If there is an active scene, dispose of it.
        s_activeScene?.Dispose();

        // Force the garbage collector to collect to ensure memory is cleared.
        GC.Collect();

        // Change the currently active scene to the new scene.
        s_activeScene = s_nextScene;

        // Null out the next scene value so it does not trigger a change over and over.
        s_nextScene = null;

        // If the active scene now is not null, initialize it.
        // Remember, just like with Game, the Initialize call also calls the
        // Scene.LoadContent
        if (s_activeScene != null)
            Profiler.Profile(s_activeScene.Initialize);
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        var path = Path.Combine(Content.RootDirectory, "fmod/Desktop");
        var ifiles = Directory.EnumerateFiles(path);
        foreach (var file in ifiles.Where(file => file.EndsWith(".bank")))
            FMOD.LoadBank(file, null);
    }

    protected override void Initialize()
    {
        FMOD = new(false);

        base.Initialize();

        // Set the core's graphics device to a reference of the base Game's
        // graphics device.
        GraphicsDevice = base.GraphicsDevice;

        // Create the sprite batch instance.
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a 1x1 white pixel texture for drawing quads.
        Pixel = new Texture2D(GraphicsDevice, 1, 1);
        Pixel.SetData([Color.White]);

        // Create the ImGui renderer.
        ImGuiRenderer = new ImGuiRenderer(this);
        ImGuiRenderer.RebuildFontAtlas();

        // Optional: Scale text and widgets for easier readability.
        var io = ImGui.GetIO();
        io.FontGlobalScale = 1.75f;
        ImGui.GetStyle().ScaleAllSizes(1.5f);

    }
}