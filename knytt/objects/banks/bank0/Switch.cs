using Godot;
using YKnyttLib;

public abstract class Switch : GDKnyttBaseObject
{
    protected KnyttSwitch @switch;

    protected string sound;
    protected bool alreadyExecuted;

    public override void _Ready()
    {
        var shape = GetNode<Node>("Shapes").GetChild<Node2D>((int)@switch.Shape);
        shape.Visible = @switch.Visible;
        shape.GetNode<Area2D>("Area2D").SetDeferred("monitoring", true);

        sound = @switch.Sound == null || @switch.Sound == "" ? "teleport" : 
                @switch.Sound.ToLower() == "none" ? null : @switch.Sound;
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni juni)) { return; }

        if (@switch.AsOne) { GDArea.Selector.Register(this, by_type: true); }
        
        if (@switch.OnTouch)
        { 
            if (@switch.DenyHologram && juni.Hologram != null)
            {
                juni.Connect(nameof(Juni.HologramStopped), this, nameof(execute));
            }
            else
            {
                execute(juni);
            }
        }
        else
        {
            juni.Connect(nameof(Juni.DownEvent), this, nameof(execute));
        }
    }

    public void _on_Area2D_body_exited(Node body)
    {
        if (!(body is Juni juni)) { return; }

        if (@switch.AsOne) { GDArea.Selector.Unregister(this, by_type: true); }

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
        if (!@switch.AsOne || GDArea.Selector.IsObjectSelected(this, by_type: true))
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
