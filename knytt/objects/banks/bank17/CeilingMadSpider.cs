using Godot;
using YUtil.Random;

public partial class CeilingMadSpider : TrapFlower
{
    public override void _Ready()
    {
        GDArea.Bullets.RegisterEmitter(this, "DropStuff",
            (p, i) =>
            {
                p.Translate(new Vector2(12, 14));
                p.VelocityMMF2 = random.Next(20);
                p.DirectionMMF2 = 24 + random.Next(1, 5) * (random.NextBoolean() ? 1 : -1);
                p.GravityMMF2 = 12;
            });
    }
}
