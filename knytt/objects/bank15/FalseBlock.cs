using Godot;

public partial class FalseBlock : GDKnyttBaseObject
{
    protected AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("Sprite2D/AnimationPlayer");
    }

    private void _on_Area2D_body_entered(Node body)
    {
        animationPlayer.Play("fade");
    }

    private void _on_Area2D_body_exited(Node body)
    {
        animationPlayer.Play("appear");
    }
}
