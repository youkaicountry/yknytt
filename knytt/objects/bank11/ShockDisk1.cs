using Godot;

public partial class ShockDisk1 : ShockDisk
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "ShockWave",
            (p, i) =>
            {
                p.Translate(new Vector2(12, 12));
                p.VelocityMMF2 = 30;
                p.DirectionMMF2 = i;
                p.DecelerationMMF2 = 20;
                p.DisappearWhenStopped = true;
            });
    }
}
