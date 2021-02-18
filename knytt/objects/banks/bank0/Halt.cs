using Godot;
using System;

public class Halt : GDKnyttBaseObject
{
    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni juni)) { return; }
        juni.CanJump = false;
        juni.Sticky = true;
    }

    public void _on_Area2D_body_exited(Node body)
    {
        if (!(body is Juni juni)) { return; }
        juni.CanJump = true;
        juni.Sticky = false;
    }
}
