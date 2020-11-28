using Godot;

public class ShockDisk3 : ShockDisk
{
    protected override void registerEmitter()
    {
        GDArea.Bullets.RegisterEmitter(this, "ShockWave2", 40,
            (p, i) => 
            {
                p.Translate(new Vector2(12, 12));
                p.VelocityMMF2 = 35;
                p.DirectionMMF2 = i;
                p.GravityMMF2 = 12;
            });
    }
}
