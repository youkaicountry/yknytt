using Godot;
using YUtil.Random;

public class SmallTimedFlower : GDKnyttBaseObject
{
    private const int SHOTS = 10;

    private int shotsLeft;

    public override void _Ready()
    {
        GDArea.Bullets.RegisterEmitter(this, "EvilFlowerBullet",
            (p, i) => 
            {
                p.DisapperarPlayer = GetNode<RawAudioPlayer2D>("HitPlayer");
                p.Translate(new Vector2(12, 18));
                p.VelocityMMF2 = 20 + random.Next(40);
                p.DirectionMMF2 = 8 + random.Next(1, 3) * (random.NextBoolean() ? 1 : -1);
                p.GravityMMF2 = 10;
                p.DecelerationMMF2 = 10;
                p.ApllyPinballCorrections();
            });
    }

    private void _on_AttackTimer_timeout_ext()
    {
        GetNode<AnimatedSprite>("AnimatedSprite").Play("open");
        shotsLeft = SHOTS;
        GetNode<TimerExt>("ShotPlayerTimer").RunTimer();
        GetNode<TimerExt>("ShotTimer").RunTimer();
    }

    private void _on_ShotTimer_timeout_ext()
    {
        GDArea.Bullets.Emit(this);
        shotsLeft--;
        if (shotsLeft == 0)
        {
            GetNode<AnimatedSprite>("AnimatedSprite").Play("open", backwards: true);
            GetNode<Timer>("ShotTimer").Stop();
            GetNode<Timer>("ShotPlayerTimer").Stop();
        }
    }
}
