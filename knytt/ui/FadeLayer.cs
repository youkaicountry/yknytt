using Godot;

public class FadeLayer : Control
{
    [Signal] public delegate void FadeDone();

    public void startFade(bool is_out = true)
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play(is_out ? "FadeOut" : "FadeIn");
    }

    public void _on_AnimationPlayer_animation_finished(string name)
    {
        EmitSignal(nameof(FadeDone));
    }
}
