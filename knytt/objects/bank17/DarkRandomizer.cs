using Godot;
using YUtil.Random;

public class DarkRandomizer : WalkingShooter
{
    public override void _Ready()
    {
        GDArea.Bullets.RegisterEmitter(this, "DropStuff",
            (p, i) =>
            {
                p.Translate(new Vector2(12, 14));
                p.VelocityMMF2 = 20 + random.Next(40);
                p.DirectionMMF2 = 8 + random.Next(1, 5) * (random.NextBoolean() ? 1 : -1);
                p.GravityMMF2 = 12;
            });
        base._Ready();
    }
}
