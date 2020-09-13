using Godot;

public class FadeLayer : CanvasLayer
{
    [Signal] delegate void FadeDone();

    public void startFade()
    {
        GetNode<AnimationPlayer>("FadeMask/AnimationPlayer").Play("FadeOut");
    }

    public void _on_AnimationPlayer_animation_finished(string name)
    {
        EmitSignal(nameof(FadeDone));
    }
}
