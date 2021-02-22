using Godot;
using YKnyttLib;

public class LocationLabel : Label
{

    public bool Fading { get; private set; } = false;

    [Export]
    float showTime = 2f;

    [Export]
    float fadeOutTime = 1.5f;

    public override void _Ready()
    {
    }

    public void toggle()
    {
        Visible = !Visible;
        showLocation();
    }

    public void updateLocation(KnyttPoint location)
    {
        this.Text = location.ToString();
        this.showLocation();
    }

    public void showLocation()
    {
        if (!Visible) { return; }
        var player = this.GetNode("AnimationPlayer") as AnimationPlayer;
        player.PlaybackSpeed = 1f / showTime;
        player.Stop();
        player.Play("FadeOut");
    }

    public void startFadeOut()
    {
        var player = this.GetNode("AnimationPlayer") as AnimationPlayer;
        player.PlaybackSpeed = 1f / fadeOutTime;
    }
}
