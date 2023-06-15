using Godot;

public class FalseBlock : GDKnyttBaseObject
{
    protected AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("Sprite/AnimationPlayer");
    }

    private void _on_Area2D_body_entered(object body)
    {
        animationPlayer.Play("fade");
    }

    private void _on_Area2D_body_exited(object body)
    {
        animationPlayer.Play("appear");
    }
}
