using Godot;

public class PausePanel : Control
{
    PackedScene settings_scene;
    PackedScene info_scene;
    bool bounce = true;
    bool in_settings = false;

    public override void _Ready()
    {
        settings_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/SettingsScreen.tscn");
        info_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/InfoScreen.tscn");
        pause();
        bounceWait();
        RectScale = Vector2.One;
        if (TouchSettings.EnablePanel) { RectScale /= TouchSettings.Viewport; }
        GetParent<BasicScreen>().initFocus();
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
        if (what == MainLoop.NotificationWmGoBackRequest) { BasicScreen.ActiveScreen.goBack(); }
    }

    private void pause()
    {
        GetTree().Paused = true;
        // second option: instead of ugly stretching of fonts, disable touch panel, and show game area without borders
        //GDKnyttSettings.setupViewport(for_ui: true);
        //GetTree().Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame")?.setupCamera(force_fullscreen: true);
        //GetTree().Root.GetNodeOrNull<TouchPanel>("GKnyttGame/UICanvasLayer/TouchPanel")?.Configure(force_off: true);
    }

    public void unpause()
    {
        GetTree().Paused = false;
        //GDKnyttSettings.setupViewport(for_ui: false);
        //GetTree().Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame")?.setupCamera(force_fullscreen: false);
        //GetTree().Root.GetNodeOrNull<TouchPanel>("GKnyttGame/UICanvasLayer/TouchPanel")?.Configure(force_off: false);
        Input.ActionRelease("pause");
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
        Input.ActionRelease("pause");
        GDKnyttSettings.setupViewport(for_ui: true);
        GetParent<BasicScreen>().loadScreen(settings_scene.Instance<SettingsScreen>());
    }

    private void _on_InfoButton_pressed()
    {
        in_settings = true;
        Input.ActionRelease("pause");
        GDKnyttSettings.setupViewport(for_ui: true);
        var info_screen = info_scene.Instance<InfoScreen>();
        info_screen.initialize(GetNode<GDKnyttGame>("../../..").GDWorld.KWorld);
        GetParent<BasicScreen>().loadScreen(info_screen);
    }

    public void backEvent()
    {
        in_settings = false;
        GDKnyttSettings.setupViewport(for_ui: false);
    }

    public void _on_QuitButton_pressed()
    {
        ClickPlayer.Play();
        Input.ActionRelease("pause");
        GetNode<GDKnyttGame>("../../..").quit();
    }
}
