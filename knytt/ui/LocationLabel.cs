using Godot;
using YKnyttLib;

public class LocationLabel : Label
{
    public bool Flash { get; set; } = true;
    public bool ShowFlags { get; set; } = false;

    [Export] float showTime = 2f;

    [Export] float fadeOutTime = 1.5f;

    private KnyttPoint location;
    private bool[] flags;

    public override void _Ready()
    {
    }

    public void toggle()
    {
        Visible = !Visible;
        showLabel();
    }

    public void updateLocation(KnyttPoint location)
    {
        this.location = location;
        this.showLabel();
    }

    public void updateFlags(bool[] flags)
    {
        this.flags = flags;
        this.showLabel();
    }

    public void showLabel()
    {
        if (!Visible) { return; }
        Text = $"x{location.x}y{location.y}";
        if (ShowFlags)
        {
            Text += $" {(flags[0]?1:0)}{(flags[1]?1:0)} {(flags[2]?1:0)}{(flags[3]?1:0)} " + 
                $"{(flags[4]?1:0)}{(flags[5]?1:0)} {(flags[6]?1:0)}{(flags[7]?1:0)} {(flags[8]?1:0)}{(flags[9]?1:0)}";
        }

        if (!Flash) { Modulate = new Color(1, 1, 1, 1); return; }
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
