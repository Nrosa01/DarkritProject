using Darkrit.Base;
using Darkrit.Content;
using Darkrit.DevTools.Logger;
using Darkrit.DevTools.Logger.Renderers;
using Darkrit.Graphics;
using Darkrit.ImGuiUtils;
using Darkrit.ImGuiUtils.Themes;
using Darkrit.InputSystem;
using Darkrit.InputSystem.Providers;
using Darkrit.Scenes;
using Darkrit.Utilities;
using ExampleMonoGame;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TinyFmod;

namespace Darkrit;

public class Core : Game
{
    // Loggers
    ImGuiLoggerConsole ImGuiLoggerConsole { get; set; }

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
    public static Input Input { get; private set; }

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

    public static Viewport Viewport { get; private set;  }

    private RenderTarget2D _sceneTarget;
    private ImTextureRef _sceneTextureId;
    
    private CoreStats _coreStats;
    
    private Point _pendingViewportSize;
    private float _resizeDelay;
    private bool _hasPendingResize;
    private const float ResizeDelaySeconds = 0.01f;

    private static bool _showEditor = true;
    private static bool _viewportFocused = false;
    private static bool IsGameNotFocused => !_viewportFocused && _showEditor;
    private static bool IsGameFocused => !IsGameNotFocused;
    public static int PHYSICS_TICKS_PER_SECOND { get; set; } = 45;

    // this will be the FixedUpdate frequency, we set it to 30 FPS
    private float fixedUpdateDelta = (int)(1000 / (float)PHYSICS_TICKS_PER_SECOND);

    // helper variables for the fixed update
    private float previousT = 0;
    private float accumulator = 0.0f;
    private float maxFrameTime = 250;

    // Elapsed time here will be fake, set to fixedUpdateDelta
    // A difference instance from the normal game time is not really needed
    // but I prefer diong the separation
    private GameTime physicsGameTime = new();


    // this value stores how far we are in the current frame. For example, when the 
    // value of ALPHA is 0.5, it means we are halfway between the last frame and the 
    // next upcoming frame.
    public static float FixedUpdateAlpha { get; private set; }  = 0;


    // Record system
    private readonly PhysicalInputProvider EngineInputProvider = new();
    private readonly ActivatableInputProvider activatableInputProvider = new(new PhysicalInputProvider());
    private readonly RecordInputProvider recordInputProvider;
    private readonly ReplayInputProvider replayInputProvider = new();

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

#if PUBLISHED
#else
        Window.AllowUserResizing = true;
#endif
        // Set the core's content manager to a reference of the base Game's
        // content manager.
        Content = base.Content;

        // Set the root directory for content.
        Content.RootDirectory = "Content";

        // Mouse is visible by default.
        IsMouseVisible = true;

        recordInputProvider = new(activatableInputProvider);

        // Create a new input manager.
        Input = new(recordInputProvider);

        var imguiLogger = new ImGuiLogger();
        ImGuiLoggerConsole = new(imguiLogger);
        Log.AddLogger(imguiLogger);

        replayInputProvider.OnPlaybackFinished += OnInputPlaybackFinished;
    }

    private void OnInputPlaybackFinished()
    {
        Input.SetProvider(recordInputProvider);
    }

    protected void FixedUpdate(GameTime gameTime)
    {
        if (previousT == 0)
        {
            previousT = (float)gameTime.TotalGameTime.TotalMilliseconds;
        }

        float now = (float)gameTime.TotalGameTime.TotalMilliseconds;
        float frameTime = now - previousT;
        if (frameTime > maxFrameTime)
        {
            frameTime = maxFrameTime;
        }

        previousT = now;

        accumulator += frameTime;

        while (accumulator >= fixedUpdateDelta)
        {
            physicsGameTime.TotalGameTime = gameTime.TotalGameTime;
            physicsGameTime.IsRunningSlowly = gameTime.IsRunningSlowly;
            physicsGameTime.ElapsedGameTime = TimeSpan.FromMilliseconds(fixedUpdateDelta);
            s_activeScene?.FixedUpdate(physicsGameTime);
            accumulator -= fixedUpdateDelta;
        }

        // this value stores how far we are in the current frame. For example, when the 
        // value of ALPHA is 0.5, it means we are halfway between the last frame and the 
        // next upcoming frame.
        FixedUpdateAlpha = (accumulator / fixedUpdateDelta);
    }

    protected override void Update(GameTime gameTime)
    {
        _coreStats.Update(gameTime);
        FMOD.Update();

        activatableInputProvider.Enabled = true;
        EngineInputProvider.Update(gameTime);

        if (EngineInputProvider.WasKeyJustPressed(Keys.F11))
            _showEditor = !_showEditor;

        if (EngineInputProvider.WasKeyJustPressed(Keys.Escape) && _viewportFocused)
            ImGui.SetWindowFocus();

        // While replaying, the game window recives input focus no matter what
        if (IsGameNotFocused && !replayInputProvider.IsReplaying)
            activatableInputProvider.Enabled = false;

        if(IsGameFocused && requestedRecording)
        {
            recordInputProvider.StartRecording();
            requestedRecording = false;
        }

        // Update the input manager.
        Input.Update(gameTime);

        if (ExitOnEscape && EngineInputProvider.WasKeyJustPressed(Keys.Escape) && IsGameNotFocused)
            Exit();

        // if there is a next scene waiting to be switch to, then transition
        // to that scene.
        if (s_nextScene != null)
            TransitionScene();

        Content.ReloadChangedAssets();

        _coreStats.ProfileStartLogic();

        // If there is an active scene, update it.
        s_activeScene?.Update(gameTime);

        FixedUpdate(gameTime);

        _coreStats.ProfileEndLogic(gameTime);

        base.Update(gameTime);
    }

    readonly IReadOnlyList<Type> _sceneTypes = ReflectionUtils.FindAllDerivedTypes<Scene>();

    bool requestedRecording = false;
    internal void EditorDraw(GameTime gameTime)
    {
        s_activeScene?.DebugDraw(gameTime);
        ImGuiLoggerConsole.Draw(gameTime);

        ImGui.Begin("Scene Switcher");

        foreach (var sceneType in _sceneTypes)
        {
            if (ImGui.Button(sceneType.Name))
                ChangeScene((Scene)Activator.CreateInstance(sceneType));
        }

        ImGui.End();

        ImGui.Begin("Input Replay");
        if (ImGuiEx.DisableButton("Record", recordInputProvider.IsRecording || requestedRecording))
            requestedRecording = true;

        if (requestedRecording && ImGui.Button("Stop recording quest"))
            requestedRecording = false;

        if(ImGuiEx.DisableButton("Stop recording", !recordInputProvider.IsRecording))
            recordInputProvider.StopRecording();

        if(ImGuiEx.DisableButton("Replay saved Input", !recordInputProvider.HasRecording))
        {
           Input.SetProvider(replayInputProvider);
           replayInputProvider.StartReplay(recordInputProvider.GetRecordedFrames());
        }

        if (recordInputProvider.IsRecording)
            ImGui.Text($"Recording Frame {recordInputProvider.RecordedFrames}");

        if (replayInputProvider.IsReplaying)
            ImGui.Text($"Replaying frame {replayInputProvider.CurrentFrame} or {replayInputProvider.TotalFrames}");
        
        ImGui.End();
    }

    protected override void Draw(GameTime gameTime)
    {
        _coreStats.ProfileStartRender();

        ImGuiRenderer.BeforeLayout(gameTime);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        s_activeScene?.Draw(gameTime);

        if (_showEditor)
        {
            RenderWithDocking(() =>
            {
                GraphicsDevice.Clear(Color.CornflowerBlue);
                s_activeScene?.Draw(gameTime);
                EditorDraw(gameTime);
                Material.DrawVisibleDebugUi();
            }, gameTime);
        }
        else
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            s_activeScene?.Draw(gameTime);
            Viewport = GraphicsDevice.Viewport;
        }

        ImGuiRenderer.AfterLayout();

        _coreStats.ProfileEndRender(gameTime);

        base.Draw(gameTime);
    }

    private void RenderWithDocking(Action renderAction, GameTime gameTime)
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

        _viewportFocused = ImGui.IsWindowFocused();

        ImGui.End();

        GraphicsDevice.SetRenderTarget(_sceneTarget);

        renderAction?.Invoke();

        GraphicsDevice.SetRenderTarget(null);

        _coreStats.DrawStats();
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
        Core.ImGuiRenderer.UnbindTexture(_sceneTextureId);


        _sceneTarget.Dispose();

        _sceneTarget = new RenderTarget2D(
            GraphicsDevice,
            size.X,
            size.Y);

        _sceneTextureId = Core.ImGuiRenderer.BindTexture(_sceneTarget);
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

        _coreStats = new(GraphicsDevice);

        //GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
        //GraphicsDevice.RasterizerState = RasterizerState.CullNone;

        // Create the sprite batch instance.
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a 1x1 white pixel texture for drawing quads.
        Pixel = new Texture2D(GraphicsDevice, 1, 1);
        Pixel.SetData([Color.White]);

        // Create the ImGui renderer.
        ImGuiRenderer = new ImGuiRenderer(this);

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

        _sceneTarget = new RenderTarget2D(
            GraphicsDevice,
            1280,
            720,
            false,
            SurfaceFormat.Color,
            DepthFormat.Depth24);

        _sceneTextureId = Core.ImGuiRenderer.BindTexture(_sceneTarget);
    }
}