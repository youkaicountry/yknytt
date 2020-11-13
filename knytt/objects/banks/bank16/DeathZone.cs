using Godot;

public class DeathZone : GDKnyttBaseObject
{
    public void _on_Area2D_body_entered(Node body)
    {
        if (body is Juni) { ((Juni)body).die(); }
    }
}
