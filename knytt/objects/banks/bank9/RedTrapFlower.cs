using Godot;

public class RedTrapFlower : TrapFlower
{
    public override void _Ready()
    {
        GDArea.Bullets.RegisterEmitter(this, "EvilFlowerBullet",
            (p, i) => 
            {
                p.Translate(new Vector2(11, 14));
                p.VelocityMMF2 = 30 + random.Next(30);
                p.DirectionMMF2 = 7 + random.Next(2) * 2;
                p.GravityMMF2 = 10;
                p.DecelerationMMF2 = 10;
                p.ApllyPinballCorrections();
            });
    }    
}
