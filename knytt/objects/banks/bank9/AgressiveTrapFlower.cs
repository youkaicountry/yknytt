using Godot;
using YUtil.Random;

public class AgressiveTrapFlower : TrapFlower
{
    protected override void registerEmitter()
    {
        GDArea.Bullets.RegisterEmitter(this, "FireBullet", 150,
            (p, i) => 
            {
                p.DisapperarPlayer = GetNode<AudioStreamPlayer2D>("HitPlayer");
                p.Translate(new Vector2(11, 12));
                p.VelocityMMF2 = 30 + GDKnyttDataStore.random.Next(30);
                p.DirectionMMF2 = 8 + GDKnyttDataStore.random.Next(1, 5) * (GDKnyttDataStore.random.NextBoolean() ? 1 : -1);
                p.GravityMMF2 = 10;
                p.DecelerationMMF2 = 10;
            });
    }    
}
