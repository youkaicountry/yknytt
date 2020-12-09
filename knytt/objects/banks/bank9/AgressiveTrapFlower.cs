using Godot;
using YUtil.Random;

public class AgressiveTrapFlower : TrapFlower
{
    protected override void registerEmitter()
    {
        GDArea.Bullets.RegisterEmitter(this, "FireBullet",
            (p, i) => 
            {
                p.DisapperarPlayer = GetNode<RawAudioPlayer2D>("HitPlayer");
                p.Translate(new Vector2(11, 12));
                p.VelocityMMF2 = 30 + random.Next(30);
                p.DirectionMMF2 = 8 + random.Next(1, 5) * (random.NextBoolean() ? 1 : -1);
                p.GravityMMF2 = 10;
                p.DecelerationMMF2 = 10;
                p.ApllyPinballCorrections();
            });
    }    
}
