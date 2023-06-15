using Godot;

public class Muffle : GDKnyttBaseObject
{
    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni juni)) { return; }
        juni.Muffle = true;
    }

    public void _on_Area2D_body_exited(Node body)
    {
        if (!(body is Juni juni)) { return; }
        juni.Muffle = false;
    }
}
