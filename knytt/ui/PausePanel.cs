using Godot;

public class PausePanel : Control
{
    PackedScene settings_scene;
    bool bounce = true;
    bool in_settings = false;

    public override void _Ready()
    {
        settings_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/SettingsScreen.tscn");
        pause();
        bounceWait();
        RectScale = Vector2.One;
        if (TouchSettings.EnablePanel) { RectScale /= TouchSettings.Viewport; }
        GetParent<BasicScreeen>().initFocus();
    }

    public async void bounceWait()
    {
        var timer = GetNode<Timer>("BounceTimer");
        timer.Start();
        await ToSignal(timer, "timeout");
        bounce = false;
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("pause") && !in_settings) { unpause(); }
    }

    public override void _Notification(int what)
    {
        if (bounce) { return; }
        if (what == MainLoop.NotificationWmGoBackRequest) { unpause(); }
    }

    private void pause()
    {
        GetTree().Paused = true;
        // second option: instead of ugly stretching of fonts, disable touch panel, and show game area without borders
        //GDKnyttSettings.setupViewport(for_ui: true);
        //(GetTree().Root.FindNode("GKnyttGame", owned: false) as GDKnyttGame)?.setupCamera(force_fullscreen: true);
        //(GetTree().Root.FindNode("TouchPanel", owned: false) as TouchPanel)?.Configure(force_off: true);
    }

    private void unpause()
    {
        GetTree().Paused = false;
        //GDKnyttSettings.setupViewport(for_ui: false);
        //(GetTree().Root.FindNode("GKnyttGame", owned: false) as GDKnyttGame)?.setupCamera(force_fullscreen: false);
        //(GetTree().Root.FindNode("TouchPanel", owned: false) as TouchPanel)?.Configure(force_off: false);
        GetParent().QueueFree();
    }

    public void _on_ResumeButton_pressed()
    {
        ClickPlayer.Play();
        unpause();
    }

    public void _on_SettingsButton_pressed()
    {
        in_settings = true;
        GDKnyttSettings.setupViewport(for_ui: true);
        GetParent<BasicScreeen>().loadScreen(settings_scene.Instance() as SettingsScreen);
    }

    public void backEvent()
    {
        in_settings = false;
        GDKnyttSettings.setupViewport(for_ui: false);
    }

    public void _on_QuitButton_pressed()
    {
        ClickPlayer.Play();
        quit();
    }

    private async void quit()
    {
        var worldInfo = GetNode<GDKnyttGame>("../../..").GDWorld.KWorld.Info;
        GetNode<RateHTTPRequest>("../RateHTTPRequest").send(worldInfo.Name, worldInfo.Author, (int)RateHTTPRequest.Action.Exit);

        var fade = GetNode<FadeLayer>("../../../FadeCanvasLayer/Fade");
        fade.startFade();
        await ToSignal(fade, "FadeDone");

        GetTree().Paused = false;
        GetTree().ChangeScene("res://knytt/ui/MainMenu.tscn");
    }
}
