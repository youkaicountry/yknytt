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

    public async void updateResolution()
    {
        bool prev_visible = Visible;
        Visible = true;
        var window_x_fixed = OS.WindowSize.x * TouchSettings.ViewportNow;
        float scale = window_x_fixed * GDKnyttSettings.Aspect / 240;
        if (!GDKnyttSettings.SideScroll) { window_x_fixed *= GDKnyttSettings.Aspect / 0.4f; }
        Text = $"{window_x_fixed:F0}x{240 * scale:F0} ({scale:F2}x)";
        
        var player = this.GetNode("AnimationPlayer") as AnimationPlayer;
        bool prev_playing = player.IsPlaying();
        player.PlaybackSpeed = 1f / showTime;
        player.Stop();
        player.Play("FadeOut");
        await ToSignal(player, "animation_finished");
        if (!prev_playing) { Visible = prev_visible; }
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
