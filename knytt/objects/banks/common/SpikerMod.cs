using Godot;

public partial class SpikerMod : DistanceMod
{
    [Export] NodePath areaPath = null;
    [Export] bool playSound = true;

    private Area2D area;
    private AudioStreamPlayer2D openPlayer;
    private AudioStreamPlayer2D closePlayer;
    private bool hologram_event_connected;

    public override void _Ready()
    {
        base._Ready();
        area = GetNode<Area2D>(areaPath);
        openPlayer = GetNode<AudioStreamPlayer2D>("OpenPlayer");
        closePlayer = GetNode<AudioStreamPlayer2D>("ClosePlayer");
        area.BodyEntered  += _body_entered;
        area.BodyExited += _body_exited;
    }

    private void _on_tree_exiting()
    {
        area.BodyEntered -= _body_entered;
        area.BodyExited -= _body_exited;
        if (hologram_event_connected) { globalJuni.HologramStopped -= hologramStopped; }
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
            juni.HologramStopped += hologramStopped;
            hologram_event_connected = true;
        }
    }

    public void _body_exited(Node body)
    {
        if (!(body is Juni juni)) { return; }
        if (hologram_event_connected)
        {
            juni.HologramStopped -= hologramStopped;
            hologram_event_connected = false;
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
