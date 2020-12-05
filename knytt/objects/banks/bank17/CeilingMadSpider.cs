using Godot;
using YUtil.Random;

public class CeilingMadSpider : TrapFlower
{
    protected override void registerEmitter()
    {
        GDArea.Bullets.RegisterEmitter(this, "DropStuff", 500,
            (p, i) => 
            {
                p.Translate(new Vector2(12, 14));
                p.VelocityMMF2 = GDKnyttDataStore.random.Next(20);
                p.DirectionMMF2 = 24 + GDKnyttDataStore.random.Next(1, 5) * (GDKnyttDataStore.random.NextBoolean() ? 1 : -1);
                p.GravityMMF2 = 12;
            });
    }    
}
