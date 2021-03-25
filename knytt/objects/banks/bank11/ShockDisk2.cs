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
                p.VelocityMMF2 = 20 + random.Next(35);
                p.DirectionMMF2 = i;
                p.DecelerationMMF2 = 20;
                p.DisappearWhenStopped = true;
            });
    }
}
