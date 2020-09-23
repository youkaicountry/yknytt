using System.Collections.Generic;
using Godot;

public class Shift : GDKnyttBaseObject
{
    HashSet<Juni> junis;

    public override void _Ready()
    {
        junis = new HashSet<Juni>();
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni)) { return; }
        junis.Add((Juni)body);
    }

    public void _on_Area2D_body_exited(Node body)
    {
        if (!(body is Juni)) { return; }
        junis.Remove((Juni)body);
    }

    protected override void _impl_initialize()
    {
        
    }

    protected override void _impl_process(float delta)
    {
        
    }
}
