using Godot;

public class RedTrapFlower : TrapFlower
{
    protected override void registerEmitter()
    {
        GDArea.Bullets.RegisterEmitter(this, "EvilFlowerBullet", 150,
            (p, i) => 
            {
                p.DisapperarPlayer = GetNode<AudioStreamPlayer2D>("HitPlayer");
                p.Translate(new Vector2(11, 14));
                p.VelocityMMF2 = 30 + GDKnyttDataStore.random.Next(30);
                p.DirectionMMF2 = 7 + GDKnyttDataStore.random.Next(2) * 2;
                p.GravityMMF2 = 10;
                p.DecelerationMMF2 = 10;
            });
    }    
}
