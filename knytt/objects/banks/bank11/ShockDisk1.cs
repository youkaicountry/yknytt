using Godot;

public class ShockDisk1 : ShockDisk
{
    protected override void registerEmitter()
    {
        GDArea.Bullets.RegisterEmitter(this, "ShockWave", 40,
            (p, i) => 
            {
                p.Translate(new Vector2(12, 12));
                p.VelocityMMF2 = 30;
                p.DirectionMMF2 = i;
                p.DecelerationMMF2 = 20;
            });
    }
}
