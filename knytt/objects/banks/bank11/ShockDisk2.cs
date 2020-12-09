using Godot;

public class ShockDisk2 : ShockDisk
{
    protected override void registerEmitter()
    {
        GDArea.Bullets.RegisterEmitter(this, "ShockWave", 40,
            (p, i) => 
            {
                p.Translate(new Vector2(12, 12));
                p.VelocityMMF2 = 20 + random.Next(35);
                p.DirectionMMF2 = i;
                p.DecelerationMMF2 = 20;
            });
    }
}
