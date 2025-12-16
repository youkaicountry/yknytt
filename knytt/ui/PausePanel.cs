using System;
using Godot;

public class PausePanel : Control
{
    PackedScene settings_scene;
    PackedScene info_scene;
    bool bounce = true;
    bool in_settings = false;
    long start_time;

    public override void _Ready()
    {
        settings_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/SettingsScreen.tscn");
        info_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/InfoScreen.tscn");
        pause();
        bounceWait();
        RectScale = Vector2.One * 0.4f / (GDKnyttSettings.Aspect * TouchSettings.ViewportNow);
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
        start_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public void unpause()
    {
        GetTree().Paused = false;
        Input.ActionRelease("pause");
        foreach (var a in Cutscene.actions) if (Input.IsActionJustPressed(a)) { Input.ActionRelease(a); }
        GetNode<GDKnyttGame>("../../..").Juni.Powers.adjustPlayingTime((int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - start_time));
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
        RectScale = Vector2.One * 0.4f / (GDKnyttSettings.Aspect * TouchSettings.ViewportNow);
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
