using Darkrit.Content;
using Darkrit.Editor;
using Darkrit.InputSystem;
using Darkrit.InputSystem.Providers;
using Darkrit.Scenes;
using Darkrit.Utilities;
using ExampleMonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using TinyFmod;

namespace Darkrit;

public class Core : Game
{
    public enum EngineUpdateLayer { UPDATE, FIXED_UPDATE }

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

    public EngineUpdateLayer InputUpdateLayer { get; set; } = EngineUpdateLayer.FIXED_UPDATE;

    public static FmodStudio FMOD { get; private set; }

    /// <summary>  
    /// Gets a runtime generated 1x1 pixel texture.  
    /// </summary>  
    public static Texture2D Pixel { get; private set; }

    /// <summary>
    /// Gets or Sets a value that indicates if the game should exit when the esc key on the keyboard is pressed.
    /// </summary>
    public static bool ExitOnEscape { get; set; }

    static CoreEditor s_coreEditor;

    public static ImGuiRenderer ImGuiRenderer => s_coreEditor.ImGuiRenderer;

    public static Viewport Viewport => s_coreEditor.Viewport;

    public static int PHYSICS_TICKS_PER_SECOND { get; set; } = 45;

    // this will be the FixedUpdate frequency in hz
    private float fixedUpdateDelta = (int)(1000.0 / PHYSICS_TICKS_PER_SECOND);

    // helper variables for the fixed update
    private readonly int _maxFixedUpdatesPerFrame = 3;
    private float previousT = 0;
    private float accumulator = 0.0f;
    private readonly float maxFrameTime = 250;

    // Elapsed time here will be fake, set to fixedUpdateDelta
    // A difference instance from the normal game time is not really needed
    // but I prefer diong the separation
    private readonly GameTime physicsGameTime = new();


    // this value stores how far we are in the current frame. For example, when the 
    // value of ALPHA is 0.5, it means we are halfway between the last frame and the 
    // next upcoming frame.
    public static float FixedUpdateAlpha { get; private set; }  = 0;


    // Input system
    private readonly PhysicalInputProvider _engineInputProvider = new();
    private readonly ActivatableInputProvider _activatableInputProvider;

    public static InputRecordingController InputRecorder;

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

        _activatableInputProvider = new(new PhysicalInputProvider());

        Input = new(_activatableInputProvider);

        InputRecorder = new(Input, _activatableInputProvider);
    }

    protected void HandleFixedUpdate(GameTime gameTime)
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

        int updatesThisFrame = 0;
        while (accumulator >= fixedUpdateDelta && updatesThisFrame < _maxFixedUpdatesPerFrame)
        {
            DoFixedUpdate(gameTime);
            accumulator -= fixedUpdateDelta;
            updatesThisFrame++;
        }

        // this value stores how far we are in the current frame. For example, when the 
        // value of ALPHA is 0.5, it means we are halfway between the last frame and the 
        // next upcoming frame.
        FixedUpdateAlpha = (accumulator / fixedUpdateDelta);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DoFixedUpdate(GameTime gameTime)
        {
            physicsGameTime.TotalGameTime = gameTime.TotalGameTime;
            physicsGameTime.IsRunningSlowly = gameTime.IsRunningSlowly;
            physicsGameTime.ElapsedGameTime = TimeSpan.FromMilliseconds(fixedUpdateDelta);
            
            if (InputUpdateLayer == EngineUpdateLayer.FIXED_UPDATE)
                Input.Update(gameTime);
            
            s_activeScene?.FixedUpdate(physicsGameTime);
        }
    }

    protected override void Update(GameTime gameTime)
    {
        FMOD.Update();

        InputAndEditorUpdate(gameTime);

        // if there is a next scene waiting to be switch to, then transition
        // to that scene.
        if (s_nextScene != null)
            TransitionScene();

        Content.ReloadChangedAssets();

        s_coreEditor.CoreStats.ProfileStartLogic();

        // If there is an active scene, update it.
        s_activeScene?.Update(gameTime);

        HandleFixedUpdate(gameTime);

        s_coreEditor.CoreStats.ProfileEndLogic(gameTime);

        base.Update(gameTime);
    }

    private void InputAndEditorUpdate(GameTime gameTime)
    {
        s_coreEditor.Update(gameTime);

        _engineInputProvider.Update(gameTime);

        if (_engineInputProvider.WasKeyJustPressed(Keys.F11))
            s_coreEditor.ToggleShow();

        if (_engineInputProvider.WasKeyJustPressed(Keys.Escape) &&
            s_coreEditor.ViewportFocused)
        {
            CoreEditor.UnFocus();
        }

        _activatableInputProvider.Enabled = true;

        if (s_coreEditor.IsGameNotFocused && !InputRecorder.IsReplaying)
            _activatableInputProvider.Enabled = false;

        if (s_coreEditor.IsGameFocused && InputRecorder.RecordingRequested)
            InputRecorder.StartRecording();

        if (ExitOnEscape &&
            _engineInputProvider.WasKeyJustPressed(Keys.Escape) &&
            s_coreEditor.IsGameNotFocused)
        {
            Exit();
        }

        if(InputUpdateLayer == EngineUpdateLayer.UPDATE)
            Input.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        s_coreEditor.Render(gameTime, s_activeScene);
        base.Draw(gameTime);
    }

    /// <summary>
    /// Queues a scene to change in the next frame after the current Update cycle
    /// If the same scene that is currently running is passed, nothing will happen
    /// </summary>
    /// <param name="next"></param>
    public static void ChangeScene(Scene next)
    {
        System.Diagnostics.Debug.Assert(s_activeScene != next);
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

        //GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
        //GraphicsDevice.RasterizerState = RasterizerState.CullNone;

        // Create the sprite batch instance.
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a 1x1 white pixel texture for drawing quads.
        Pixel = new Texture2D(GraphicsDevice, 1, 1);
        Pixel.SetData([Color.White]);

#if EDITOR_BUILD
        s_coreEditor = new(GraphicsDevice, new ImGuiRenderer(this));
#else
    s_coreEditor = new(GraphicsDevice, null);
#endif
    }
}