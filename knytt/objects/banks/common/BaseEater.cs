using Godot;

public partial class BaseEater : GDKnyttBaseObject
{
    private void _on_Area2D_body_entered(Node2D body)
    {
        if (!(body is Juni juni)) { return; }
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("eat");
        GetNode<AudioStreamPlayer2D>("Player").Play();
        juniDie(juni);
    }
}
