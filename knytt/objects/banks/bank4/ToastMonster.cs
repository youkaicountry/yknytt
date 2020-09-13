using Godot;
using YUtil.Random;

public class ToastMonster : GDKnyttBaseObject
{
    bool spiked = false;

    AnimationPlayer anim;

    const float SPIKE_START_DIST = 84f;
    const float SPIKE_STOP_DIST = 114f;

    public override void _Ready()
    {
        anim = GetNode<AnimationPlayer>("Sprite/AnimationPlayer");
        blink();
    }

    protected override void _impl_initialize()
    {
        OrganicEnemy = true;
    }

    protected override void _impl_process(float delta)
    {
        if (anim.IsPlaying() && anim.CurrentAnimation.Equals("Spike")) { return; }
        float dist = Juni.manhattanDistance(GlobalPosition);
        if (!spiked && dist < SPIKE_START_DIST) { startSpikes(); }
        else if (spiked && dist >= SPIKE_STOP_DIST) { stopSpikes(); }
    }

    public void startSpikes()
    {
        spiked = true;
        GetNode<AudioStreamPlayer2D>("SpikeUpPlayer2D").Play();
        anim.Play("Spike");
    }

    public void stopSpikes()
    {
        spiked = false;
        GetNode<AudioStreamPlayer2D>("SpikeDownPlayer2D").Play();
        anim.Play("Spike", customSpeed:-1f, fromEnd:true);
    }

    public async void blink()
    {
        if (spiked) { return; }
        GetNode<Sprite>("Sprite").Frame = 0;
        var timer = GetNode<Timer>("BlinkTimer");
        timer.WaitTime = GDKnyttDataStore.random.NextFloat(1f, 3f);
        timer.Start();
        await ToSignal(timer, "timeout");
        if (spiked) { return; }
        anim.Play("Blink");
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (!spiked || !(body is Juni)) { return; }
        Juni.die();
    }
}
