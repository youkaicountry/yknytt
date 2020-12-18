using Godot;
using YUtil.Random;

public class FlySpike : GDKnyttBaseObject
{
    AnimatedSprite animated;
    Timer wait_timer;
    CosineMod mod;
    public bool FacingRight { get { return !animated.FlipH; } set { animated.FlipH = !value; } }

    private const float MIN_WAIT = 1f;
    private const float MAX_WAIT = 4f;
    private const float SPEED = 192f;

    public override void _Ready()
    {
        animated = GetNode<AnimatedSprite>("AnimatedSprite");
        wait_timer = GetNode<Timer>("WaitTimer");
        mod = GetNode<CosineMod>("CosineMod");
        if (ObjectID.y == 21) 
        { 
            animated.Animation = "1";
            FacingRight = false;
        }
        start();
    }

    private async void stop()
    {
        mod.stop();
        animated.Stop();
        animated.Frame = 0;
        FacingRight = !FacingRight;
        wait_timer.WaitTime = random.NextFloat(MIN_WAIT, MAX_WAIT);
        wait_timer.Start();
        await ToSignal(wait_timer, "timeout");
        start();
    }

    private void start()
    {
        animated.Play();
        mod.start();
    }

    public override void _PhysicsProcess(float delta)
    { 
        base._PhysicsProcess(delta);
        if (wait_timer.TimeLeft > 0f) { return; }
        Translate(new Vector2(SPEED * delta * (FacingRight ? 1f : -1f), 0f));
    }

    public void _on_WallHitArea_body_entered(Node body)
    {
        if (wait_timer.TimeLeft > 0f) { return; }
        stop();
    }
}
