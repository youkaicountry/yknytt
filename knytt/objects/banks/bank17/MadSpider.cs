using Godot;
using YUtil.Random;

public class MadSpider : TrapFlower
{
    public override void _Ready()
    {
        GDArea.Bullets.RegisterEmitter(this, "DropStuff",
            (p, i) => 
            {
                p.Translate(new Vector2(12, 10));
                p.VelocityMMF2 = 30 + random.Next(30);
                p.DirectionMMF2 = 8 + random.Next(1, 5) * (random.NextBoolean() ? 1 : -1);
                p.GravityMMF2 = 12;
            });
    }    
}
