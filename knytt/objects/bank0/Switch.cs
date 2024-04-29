using Godot;
using YKnyttLib;

public abstract class Switch : GDKnyttBaseObject
{
    protected KnyttSwitch @switch;

    protected string sound;
    public bool alreadyExecuted;

    public override void _Ready()
    {
        var shape = GetNode<Node>("Shapes").GetChild<Node2D>((int)@switch.Shape);
        shape.Visible = @switch.Visible;
        shape.GetNode<Area2D>("Area2D").SetDeferred("monitoring", GDArea.Area.ExtraData != null);

        var sound_cmp = @switch.Sound?.ToLower();
        sound = sound_cmp == null || sound_cmp == "" ? "teleport" :
                sound_cmp == "none" || sound_cmp == "false" ? null : @switch.Sound;
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
                juni.Connect(nameof(Juni.HologramStopped), this, nameof(execute));
            }
            else
            {
                if (GDArea.Selector.GetSize(this) > 1) { return; }
                execute(juni);
            }
        }
        else
        {
            if (juni.juniInput.DownHeld && !juni.juniInput.SwitchHeld) { execute(juni); }
            else { juni.Connect(nameof(Juni.DownEvent), this, nameof(execute)); }
        }
    }

    public void _on_Area2D_body_exited(Node body)
    {
        if (@switch == null) { return; }
        if (!(body is Juni juni)) { return; }

        if (@switch.AsOne) { GDArea.Selector.Unregister(this); }

        if (!@switch.OnTouch && juni.IsConnected(nameof(Juni.DownEvent), this, nameof(execute)))
        {
            juni.Disconnect(nameof(Juni.DownEvent), this, nameof(execute));
        }

        if (@switch.DenyHologram && juni.IsConnected(nameof(Juni.HologramStopped), this, nameof(execute)))
        {
            juni.Disconnect(nameof(Juni.HologramStopped), this, nameof(execute));
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

        if (@switch.AsOne) { foreach (Switch s in GDArea.Objects.findObjects(ObjectID)) { s.alreadyExecuted = true; } }
        else { alreadyExecuted = true; }

        CallDeferred("_execute", juni);
    }

    protected abstract void _execute(Juni juni);
}
