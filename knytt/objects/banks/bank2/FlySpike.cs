using Godot;
using YUtil.Random;

public partial class FlySpike : GDKnyttBaseObject
{
    AnimatedSprite2D animated;
    Timer wait_timer;
    [Export] public bool FacingRight = true;

    private const float MIN_WAIT = 1.6f;
    private const float MAX_WAIT = 3.2f;
    private float speed = 200;

    public override void _Ready()
    {
        animated = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        wait_timer = GetNode<Timer>("WaitTimer");
        animated.FlipH = !FacingRight;
        start();
    }

    private async void stop()
    {
        FacingRight = !FacingRight;
        animated.Stop();
        animated.Frame = 0;
        animated.FlipH = !FacingRight;
        wait_timer.WaitTime = random.NextFloat(MIN_WAIT, MAX_WAIT);
        wait_timer.Start();
        await ToSignal(wait_timer, "timeout");
        speed = (3 + random.Next(2)) * 50;
        start();
    }

    private void start()
    {
        animated.Play();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (wait_timer.TimeLeft > 0f) { return; }
        Translate(new Vector2(speed * (float)delta * (FacingRight ? 1f : -1f), 0f));
    }

    public void _on_WallHitArea_body_entered(Node body)
    {
        if (wait_timer.TimeLeft > 0f) { return; }
        stop();
    }
}
