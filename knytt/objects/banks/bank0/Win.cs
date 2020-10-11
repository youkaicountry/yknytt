using Godot;
using System;

public class Win : GDKnyttBaseObject
{
    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni)) { return; }

        string ending = GDArea.Area.getExtraData("Ending");
        Juni.win(ending == null ? "Ending" : ending);
    }
}
