using Godot;

public class DarkHomingCannon : Cannon
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "DropStuff",
            (p, i) =>
            {
                p.Translate(new Vector2(8f, 10f));
                p.DirectionMMF2 = 12;
                p.VelocityMMF2 = Mathf.Max(0, Center.x - Juni.ApparentPosition.x) / 10f + 36f + random.Next(3);
                p.GravityMMF2 = 20 + random.Next(3);
            });
    }
}
