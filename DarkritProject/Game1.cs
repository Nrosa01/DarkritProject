using Darkrit.Scenes;
using Gum.Forms;
using Gum.Forms.Controls;
using MonoGameGum;


namespace Darkrit;

public class Game1 : Core
{
    public Game1() : base("Darkrit Project", 1280, 720, false)
    {

    }

    private void InitializeGum()
    {
        // Initialize the Gum service. The second parameter specifies
        // the version of the default visuals to use. V3 is the latest
        // version.
        GumService.Default.Initialize(this, DefaultVisualsVersion.V3);

        // Tell the Gum service which content manager to use. We will tell it to
        // use the global content manager from our Core.
        GumService.Default.ContentLoader.XnaContentManager = Core.Content;

        // Register keyboard input for UI control.
        FrameworkElement.KeyboardsForUiControl.Add(GumService.Default.Keyboard);

        // Register gamepad input for Ui control.
        FrameworkElement.GamePadsForUiControl.AddRange(GumService.Default.Gamepads);

        // Customize the tab reverse UI navigation to also trigger when the keyboard
        // Up arrow key is pushed.
        FrameworkElement.TabReverseKeyCombos.Add(
           new KeyCombo() { PushedKey = Gum.Forms.Input.Keys.Up });

        // Customize the tab UI navigation to also trigger when the keyboard
        // Down arrow key is pushed.
        FrameworkElement.TabKeyCombos.Add(
           new KeyCombo() { PushedKey = Gum.Forms.Input.Keys.Down });

        GumService.Default.CanvasWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        GumService.Default.CanvasHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
        //GumService.Default.Renderer.Camera.Zoom = 4.0f;
    }

    protected override void Initialize()
    {
        base.Initialize();

        // Initialize the Gum UI service
        InitializeGum();

        // Start the game with the title scene.
        ChangeScene(new TestSceneInput());
    }
}
