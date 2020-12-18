using Godot;

public class SpikerMod : DistanceMod
{
    [Export] NodePath areaPath = null;
    [Export] bool playSound = true;

    private Area2D area;
    private AudioStreamPlayer2D openPlayer;
    private AudioStreamPlayer2D closePlayer;

    public override void _Ready()
    {
        base._Ready();
        area = GetNode<Area2D>(areaPath);
        openPlayer = GetNode<AudioStreamPlayer2D>("OpenPlayer");
        closePlayer = GetNode<AudioStreamPlayer2D>("ClosePlayer");
        area.Connect("body_entered", this, nameof(_body_entered));
        area.Connect("body_exited", this, nameof(_body_exited));
    }

    public void _body_entered(Node body)
    {
        if (!(body is Juni juni)) { return; }
        if (IsEntered)
        {
            parent.juniDie(juni);
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
}
