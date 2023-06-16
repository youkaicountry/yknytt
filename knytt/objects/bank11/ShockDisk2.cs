using Godot;

public class ShockDisk2 : ShockDisk
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "ShockWave",
            (p, i) =>
            {
                p.Translate(new Vector2(12, 12));
                p.VelocityMMF2 = 15 + random.Next(35); // 20 + random(35) in original
                p.DirectionMMF2 = i;
                p.DecelerationMMF2 = 12; // 20 in original
                p.DisappearWhenStopped = true;
            });
    }
}
