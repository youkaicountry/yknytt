using Godot;

public partial class ShadowGirl : Muff
{
    private bool visible = true;

    protected override void changeSpeed(float s)
    {
        base.changeSpeed(s);
        var player = GetNode<AnimationPlayer>("AnimatedSprite2D/AnimationPlayer");
        if (!visible && s > 0) { player.Play("appear"); visible = true; }
        if (visible && s == 0) { player.Play("disappear"); visible = false; }
    }
}
