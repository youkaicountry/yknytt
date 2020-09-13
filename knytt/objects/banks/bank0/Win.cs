using Godot;
using System;

public class Win : GDKnyttBaseObject
{
    protected override void _impl_initialize() { }

    protected override void _impl_process(float delta) { }

    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni)) { return; }
        Juni.win();
    }
}
