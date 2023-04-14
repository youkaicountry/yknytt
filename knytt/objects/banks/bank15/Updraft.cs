using Godot;

public partial class Updraft : GDKnyttBaseObject
{
    public override void _Ready()
    {
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Frame = random.Next(12);
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni)) { return; }
        ((Juni)body).InUpdraft = true;
    }

    public void _on_Area2D_body_exited(Node body)
    {
        if (!(body is Juni)) { return; }
        ((Juni)body).InUpdraft = false;
    }
}
