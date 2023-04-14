using Godot;
using YUtil.Random;

public partial class WaterEye : BaseWaterMonster
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "DropStuff",
            (p, i) =>
            {
                p.Translate(new Vector2(12, 8));
                p.VelocityMMF2 = 20 + random.Next(40);
                p.DirectionMMF2 = 8 + random.Next(1, 4) * (random.NextBoolean() ? 1 : -1);
                p.GravityMMF2 = 12;
            });
    }
}
