using Godot;

public class SpikerMod : DistanceMod
{
    [Export] NodePath areaPath = null;
    [Export] bool playSound = true;
    [Export] bool delayDeath = false;

    private Area2D area;
    private AudioStreamPlayer2D openPlayer;
    private AudioStreamPlayer2D closePlayer;
    private bool delayedDeath;

    public override void _Ready()
    {
        base._Ready();
        area = GetNode<Area2D>(areaPath);
        openPlayer = GetNode<AudioStreamPlayer2D>("OpenPlayer");
        closePlayer = GetNode<AudioStreamPlayer2D>("ClosePlayer");
        area.Connect("body_entered", this, nameof(_body_entered));
        area.Connect("body_exited", this, nameof(_body_exited));
        sprite.Connect("animation_finished", this, nameof(_animation_finished));
    }

    public void _body_entered(Node body)
    {
        if (!(body is Juni juni)) { return; }
        if (IsEntered)
        {
            bool is_opening = delayDeath && sprite.Animation == openAnimation && 
                sprite.Frame < sprite.Frames.GetFrameCount(openAnimation) - 1; // sprite.Playing doesn't work
            if (!is_opening) { parent.juniDie(juni); } else { delayedDeath = true; }
        }
        else if (juni.Hologram != null)
        {
            juni.Connect(nameof(Juni.HologramStopped), this, nameof(hologramStopped));
        }
    }

    public void _body_exited(Node body)
    {
        if (!(body is Juni juni)) { return; }
        if (juni.IsConnected(nameof(Juni.HologramStopped), this, nameof(hologramStopped)))
        {
            juni.Disconnect(nameof(Juni.HologramStopped), this, nameof(hologramStopped));
        }
        delayedDeath = false;
    }

    public void hologramStopped(Juni juni)
    {
        updateSpikes(show: true);
        parent.juniDie(juni);
    }

    protected override void updateSpikes(bool show)
    {
        base.updateSpikes(show);
        if (playSound) { (show ? openPlayer : closePlayer).Play(); }
    }

    protected void _animation_finished()
    {
        if (delayedDeath && sprite.Frame > 0 && sprite.Animation == openAnimation) { parent.juniDie(globalJuni); }
    }
}
