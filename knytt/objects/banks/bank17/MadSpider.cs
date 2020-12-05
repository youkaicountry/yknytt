using Godot;
using YUtil.Random;

public class MadSpider : TrapFlower
{
    protected override void registerEmitter()
    {
        GDArea.Bullets.RegisterEmitter(this, "DropStuff", 500,
            (p, i) => 
            {
                p.Translate(new Vector2(12, 10));
                p.VelocityMMF2 = 30 + GDKnyttDataStore.random.Next(30);
                p.DirectionMMF2 = 8 + GDKnyttDataStore.random.Next(1, 5) * (GDKnyttDataStore.random.NextBoolean() ? 1 : -1);
                p.GravityMMF2 = 12;
            });
    }    
}
