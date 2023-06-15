using Godot;

public class BaseEater : GDKnyttBaseObject
{
    private void _on_Area2D_body_entered(object body)
    {
        if (!(body is Juni juni)) { return; }
        GetNode<AnimatedSprite>("AnimatedSprite").Play("eat");
        GetNode<AudioStreamPlayer2D>("Player").Play();
        juniDie(juni);
    }
}
