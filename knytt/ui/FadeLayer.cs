using Godot;

public class FadeLayer : Control
{
    [Signal] public delegate void FadeDone();

    public void startFade(bool is_out = true, Color? color = null)
    {
        Modulate = color ?? new Color(1, 1, 1);
        GetNode<AnimationPlayer>("Fade/AnimationPlayer").Play(is_out ? "FadeOut" : "FadeIn");
    }

    public void _on_AnimationPlayer_animation_finished(string name)
    {
        Modulate = new Color(1, 1, 1, 0);
        EmitSignal(nameof(FadeDone));
    }
}
