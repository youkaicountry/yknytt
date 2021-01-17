using System.Collections.Generic;

public class Automation : GDKnyttBaseObject
{
    private static Dictionary<int, string> actionNames = new Dictionary<int, string>()
    {
        [248] = "hologram", [249] = "walk", [250] = "jump", [251] = "umbrella", 
        [252] = "up", [253] = "down", [254] = "left", [255] = "right"
    };

    private string action;

    public override void _Ready()
    {
        GDArea.HasAltInput = true;
        action = actionNames[ObjectID.y];
    }

    private void _on_Area2D_body_entered(Juni juni)
    {
        if (GDArea.Selector.GetSize(this) == 0 && GDArea.Selector.IsOpen)
        {
            juni.altInput.ActionPress(action);
        }

        GDArea.Selector.Register(this);
    }

    private void _on_Area2D_body_exited(Juni juni)
    {
        GDArea.Selector.Unregister(this);

        if (GDArea.Selector.GetSize(this) == 0 && GDArea.Selector.IsOpen)
        {
            juni.altInput.ActionRelease(action);
        }
    }
}
