using Godot;

public class TimedFlower : GDKnyttBaseObject
{
    private bool isShooting = false;

    public override void _Ready()
    {
        GDArea.Bullets.RegisterEmitter(this, "EvilFlowerBullet",
            (p, i) =>
            {
                p.Translate(new Vector2(11, 13));
                p.VelocityMMF2 = 30 + random.Next(30);
                p.DirectionMMF2 = 7 + random.Next(2) * 2;
                p.GravityMMF2 = 10;
                p.DecelerationMMF2 = 20;
                p.ApllyPinballCorrections();
            });
    }

    private void _on_ToggleTimer_timeout_ext()
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
