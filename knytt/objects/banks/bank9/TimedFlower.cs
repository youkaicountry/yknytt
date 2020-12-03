using Godot;
using YUtil.Random;

public class TimedFlower : GDKnyttBaseObject
{
    private bool isShooting = false;

    public override void _Ready()
    {
        GDArea.Bullets.RegisterEmitter(this, "EvilFlowerBullet", 150,
            (p, i) => 
            {
                p.DisapperarPlayer = GetNode<RawAudioPlayer2D>("HitPlayer");
                p.Translate(new Vector2(11, 13));
                p.VelocityMMF2 = 30 + GDKnyttDataStore.random.Next(30);
                p.DirectionMMF2 = 7 + GDKnyttDataStore.random.Next(2) * 2;
                p.GravityMMF2 = 10;
                p.DecelerationMMF2 = 20;
                p.ApllyPinballCorrections();
            });
        var first_delay = GDArea.Selector.GetRandomValue(this, GetNode<Timer>("ToggleTimer").WaitTime);
        GetNode<Timer>("FirstDelayTimer").Start(first_delay);
    }

    private void _on_FirstDelayTimer_timeout()
    {
        GetNode<Timer>("ToggleTimer").Start();
        _on_ToggleTimer_timeout();
    }

    private void _on_ToggleTimer_timeout()
    {
        isShooting = !isShooting;
        if (isShooting)
        {
            GetNode<Timer>("ShotTimer").Start();
        }
        else
        {
            GetNode<Timer>("ShotTimer").Stop();
        }
        GetNode<AnimatedSprite>("AnimatedSprite").Play("open", backwards: !isShooting);
    }

    private void _on_ShotTimer_timeout()
    {
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
        GDArea.Bullets.Emit(this);
    }
}
