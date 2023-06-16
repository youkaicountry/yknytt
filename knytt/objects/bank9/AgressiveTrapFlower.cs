using Godot;
using YUtil.Random;

public class AgressiveTrapFlower : TrapFlower
{
    public override void _Ready()
    {
        GDArea.Bullets.RegisterEmitter(this, "FireBulletLite",
            (p, i) =>
            {
                p.Translate(new Vector2(11, 12));
                p.VelocityMMF2 = 30 + random.Next(30);
                p.DirectionMMF2 = 8 + random.Next(1, 5) * (random.NextBoolean() ? 1 : -1);
                p.GravityMMF2 = 10;
                p.DecelerationMMF2 = 10;
                p.PinballCorrection = true;
            });
    }
}
