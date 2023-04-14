using Godot;
using YKnyttLib;

public abstract partial class Switch : GDKnyttBaseObject
{
    protected KnyttSwitch @switch;

    protected string sound;
    protected bool alreadyExecuted;
    private bool hologram_event_connected;
    private bool down_event_connected;

    public override void _Ready()
    {
        var shape = GetNode<Node>("Shapes").GetChild<Node2D>((int)@switch.Shape);
        shape.Visible = @switch.Visible;
        shape.GetNode<Area2D>("Area2D").SetDeferred("monitoring", true);

        var sound_cmp = @switch.Sound?.ToLower();
        sound = sound_cmp == null || sound_cmp == "" ? "teleport" :
                sound_cmp == "none" || sound_cmp == "false" ? null : @switch.Sound;
    }

    private void _on_tree_exiting()
    {
        if (hologram_event_connected) { Juni.HologramStopped -= execute; }
        if (down_event_connected) { Juni.DownEvent -= execute; }
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (@switch == null) { return; }
        if (!(body is Juni juni)) { return; }

        if (@switch.AsOne) { GDArea.Selector.Register(this); } // TODO: can stop work if different Junis enter different shifts

        if (@switch.OnTouch)
        {
            if (@switch.DenyHologram && juni.Hologram != null)
            {
                juni.HologramStopped += execute;
                hologram_event_connected = true;
            }
            else
            {
                execute(juni);
            }
        }
        else
        {
            juni.DownEvent += execute;
            down_event_connected = true;
        }
    }

    public void _on_Area2D_body_exited(Node body)
    {
        if (@switch == null) { return; }
        if (!(body is Juni juni)) { return; }

        if (@switch.AsOne) { GDArea.Selector.Unregister(this); }

        if (!@switch.OnTouch && down_event_connected)
        {
            juni.DownEvent -= execute;
            down_event_connected = false;
        }

        if (@switch.DenyHologram && hologram_event_connected)
        {
            juni.HologramStopped -= execute;
            hologram_event_connected = false;
        }
    }

    public void execute(Juni juni)
    {
        if (!@switch.AsOne || GDArea.Selector.IsObjectSelected(this))
        {
            executeAnyway(juni);
        }
    }

    public void executeAnyway(Juni juni)
    {
        if (!@switch.Repeat && alreadyExecuted) { return; }
        alreadyExecuted = true;
        CallDeferred("_execute", juni);
    }

    protected abstract void _execute(Juni juni);
}
