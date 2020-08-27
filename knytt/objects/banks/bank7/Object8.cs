using Godot;
using System;

public class Object8 : GDKnyttBaseObject
{
    protected override void _impl_initialize()
    {
        GD.Print("Rain!");
    }

    protected override void _impl_process(float delta)
    {
    }
}
