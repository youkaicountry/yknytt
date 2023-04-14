using Godot;

public partial class DarkHomingCannon : Cannon
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "DropStuff",
            (p, i) =>
            {
                p.Translate(new Vector2(8f, 8f));
                p.DirectionMMF2 = 12;
                p.VelocityMMF2 = Mathf.Max(0, Center.X - Juni.ApparentPosition.X) / 10f + 36f + random.Next(3);
                p.GravityMMF2 = 20 + random.Next(3);
            });
    }
}
