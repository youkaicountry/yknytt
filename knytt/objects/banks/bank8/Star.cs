using Godot;
using YUtil.Random;

public class Star : GDKnyttBaseObject
{
    const float MIN_SLEEP = 2f;
    const float MAX_SLEEP = 10f;

    public override void _Ready()
    {
        startSleep();
    }

    private void startSleep()
    {
        GetNode<AnimatedSprite>("AnimatedSprite").Visible = false;
        var timer = GetNode<Timer>("SleepTimer");
        timer.WaitTime = random.NextFloat(MIN_SLEEP, MAX_SLEEP);
        timer.Start();
    }

    private void startAnimation()
    {
        var anim = GetNode<AnimatedSprite>("AnimatedSprite");
        anim.Position = new Vector2(random.NextFloat(24f), random.NextFloat(24f));

        anim.Visible = true;
        anim.Stop();
        anim.Frame = 0;
        anim.Play(""+random.Next(0, 4));
    }

    public void _on_AnimatedSprite_animation_finished()
    {
        startSleep();
    }

    public void _on_SleepTimer_timeout()
    {
        startAnimation();
    }

}
