using Godot;
using YUtil.Random;

public class HomingFlower : GDKnyttBaseObject
{
    public override void _Ready()
    {
        GDArea.Bullets.RegisterEmitter(this, "HomingBullet", 15,
            (p, i) => 
            {
                (p as HomingBullet).globalJuni = Juni;
                p.DisapperarPlayer = GetNode<AudioStreamPlayer2D>("HitPlayer");
                p.Translate(new Vector2(11, 16));
                p.Velocity = 50;
                p.Direction = Mathf.Pi / 2;
            });

        var first_delay = GDArea.Selector.GetRandomValue(this, GetNode<Timer>("LoopTimer").WaitTime);
        GetNode<Timer>("FirstDelayTimer").Start(first_delay);
    }

    private void _on_FirstDelayTimer_timeout()
    {
        GetNode<Timer>("LoopTimer").Start();
        _on_LoopTimer_timeout();
    }

    private void _on_LoopTimer_timeout()
    {
        GetNode<AnimatedSprite>("AnimatedSprite").Play("prepare");
        GetNode<Timer>("ShotTimer").Start();
    }

    private void _on_ShotTimer_timeout()
    {
        GetNode<AnimatedSprite>("AnimatedSprite").Play("close");
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
        GDArea.Bullets.Emit(this);
    }
}
