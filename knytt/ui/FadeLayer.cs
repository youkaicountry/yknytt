using Godot;

public class FadeLayer : Node
{
    [Signal] delegate void FadeDone();

    public void startFade()
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("FadeOut");
    }

    public void _on_AnimationPlayer_animation_finished(string name)
    {
        EmitSignal(nameof(FadeDone));
    }
}
