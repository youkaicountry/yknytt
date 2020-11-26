using Godot;
using YUtil.Random;

public class SmallTimedFlower : GDKnyttBaseObject
{
    private const int SHOTS = 10;

    private int shotsLeft;

    public override void _Ready()
    {
        GDArea.Bullets.RegisterEmitter(this, "EvilFlowerBullet", 150,
            (p, i) => 
            {
                p.DisapperarPlayer = GetNode<AudioStreamPlayer2D>("HitPlayer");
                p.Translate(new Vector2(12, 18));
                p.VelocityMMF2 = 20 + GDKnyttDataStore.random.Next(40);
                p.DirectionMMF2 = 8 + GDKnyttDataStore.random.Next(1, 3) * (GDKnyttDataStore.random.NextBoolean() ? 1 : -1);
                p.GravityMMF2 = 10;
                p.DecelerationMMF2 = 10;
            });
    }

    private void _on_FirstDelayTimer_timeout()
    {
        GetNode<Timer>("AttackTimer").Start();
        _on_AttackTimer_timeout();
    }

    private void _on_AttackTimer_timeout()
    {
        GetNode<AnimatedSprite>("AnimatedSprite").Play("open");
        shotsLeft = SHOTS;
        GetNode<Timer>("ShotPlayerTimer").Start();
        _on_ShotTimer_timeout();
        GetNode<Timer>("ShotTimer").Start();
        _on_ShotPlayerTimer_timeout();
    }

    private void _on_ShotTimer_timeout()
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

    private void _on_ShotPlayerTimer_timeout()
    {
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
    }
}
