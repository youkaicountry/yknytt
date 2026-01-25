using Godot;

public partial class ShockDisk3 : ShockDisk
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "ShockWave2",
            (p, i) =>
            {
                p.Translate(new Vector2(12, 12));
                p.VelocityMMF2 = 35;
                p.DirectionMMF2 = i;
                p.GravityMMF2 = 12;
            });
    }
}
