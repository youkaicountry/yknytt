using System.Collections.Generic;
using Godot;

public class Automation : GDKnyttBaseObject
{
    private static Dictionary<int, string> actionNames = new Dictionary<int, string>()
    {
        [248] = "hologram", [249] = "walk", [250] = "jump", [251] = "umbrella",
        [252] = "up", [253] = "down", [254] = "left", [255] = "right"
    };

    private string action;

    protected Juni juni;

    public override void _Ready()
    {
        GDArea.HasAltInput = true;
        action = actionNames[ObjectID.y];
    }

    protected virtual void _on_Area2D_body_entered(Juni juni)
    {
        if (!juni.juniInput.altInput.IsActionPressed(action) && GDArea.Selector.IsOpen)
        {
            juni.juniInput.altInput.ActionPress(action);
            if (action == "left" && juni.juniInput.altInput.IsActionPressed("right")) { juni.juniInput.altInput.ActionRelease("right"); }
            if (action == "right" && juni.juniInput.altInput.IsActionPressed("left")) { juni.juniInput.altInput.ActionRelease("left"); }
        }

        GDArea.Selector.Register(this);
        this.juni = juni;
    }

    protected virtual void _on_Area2D_body_exited(Juni juni)
    {
        GDArea.Selector.Unregister(this);
        this.juni = null;

        if (GDArea.Selector.GetSize(this) == 0 && GDArea.Selector.IsOpen)
        {
            juni.juniInput.altInput.ActionRelease(action);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (juni != null && (action == "left" || action == "right") && 
            !juni.juniInput.altInput.IsActionPressed("left") && 
            !juni.juniInput.altInput.IsActionPressed("right"))
        {
            juni.juniInput.altInput.ActionPress(action);
        }
    }
}
