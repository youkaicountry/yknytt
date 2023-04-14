using Godot;

public partial class FadeLayer : Control
{
    [Signal] public delegate void FadeDoneEventHandler();

    private bool reset;

    public void startFade(bool is_out = true, bool fast = false, Color? color = null, bool reset=true)
    {
        Modulate = color ?? new Color(1, 1, 1);
        GetNode<AnimationPlayer>("Fade/AnimationPlayer").Play(!is_out ? "FadeIn" : fast ? "FastFadeOut" : "FadeOut");
        this.reset = reset;
    }

    public void _on_AnimationPlayer_animation_finished(string name)
    {
        EmitSignal(SignalName.FadeDone);
        //var timer = GetNode<Timer>("ResetTimer");
        //timer.Start();
        //await ToSignal(timer, "timeout");
        if (reset) { Modulate = new Color(1, 1, 1, 0); }
    }
}
