using Godot;
using YUtil.Random;

public class WaterEye : BaseWaterMonsterNew
{
    protected override void registerEmitter()
    {
        GDArea.Bullets.RegisterEmitter(this, "DropStuff", 500,
            (p, i) => 
            {
                p.Translate(new Vector2(12, 8));
                p.VelocityMMF2 = 20 + GDKnyttDataStore.random.Next(40);
                p.DirectionMMF2 = 8 + GDKnyttDataStore.random.Next(1, 4) * (GDKnyttDataStore.random.NextBoolean() ? 1 : -1);
                p.GravityMMF2 = 12;
            });
    }
}