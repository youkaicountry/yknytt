using Godot;

public class PausePanel : Control
{
    PackedScene settings_scene;
    bool bounce = true;

    public override void _Ready()
    {
        settings_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/SettingsScreen.tscn");
        pause();
        bounceWait();
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
        if (Input.IsActionJustPressed("pause")) { unpause(); }
    }

    public override void _Notification(int what)
    {
        if (bounce) { return; }
        if (what == MainLoop.NotificationWmGoBackRequest) { unpause(); }
    }

    private void pause()
    {
        GetTree().Paused = true;
    }

    private void unpause()
    {
        GetTree().Paused = false;
        GetParent().QueueFree();
    }

    public void _on_ResumeButton_pressed()
    {
        ClickPlayer.Play();
        unpause();
    }

    public void _on_SettingsButton_pressed()
    {
        ClickPlayer.Play();
        var settings_node = this.settings_scene.Instance() as SettingsScreen;
        GetParent().AddChild(settings_node);
    }

    public void _on_QuitButton_pressed()
    {
        ClickPlayer.Play();
        quit();
    }

    private async void quit()
    {
        var fade = GetNode<FadeLayer>("../../../FadeCanvasLayer/Fade");
        fade.startFade();
        await ToSignal(fade, "FadeDone");
        GetTree().Paused = false;
        GetTree().ChangeScene("res://knytt/ui/MainMenu.tscn");
    }
}
