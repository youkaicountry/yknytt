using Godot;
using YUtil.Random;

public partial class Bubble : GDKnyttBaseObject
{
    const float MIN_DELAY = 4f;
    const float MAX_DELAY = 10f;

    public override void _Ready()
    {
        base._Ready();
        startTimer(0f, MAX_DELAY);
    }

    public void _on_DelayTimer_timeout()
    {
        GetNode<CpuParticles2D>("CPUParticles2D").Emitting = true;
        startTimer();
    }

    private void startTimer(float min = MIN_DELAY, float max = MAX_DELAY)
    {
        var timer = GetNode<Timer>("DelayTimer");
        timer.WaitTime = random.NextFloat(min, max);
        timer.Start();
    }
}
