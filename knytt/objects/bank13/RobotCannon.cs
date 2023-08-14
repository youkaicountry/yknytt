using Godot;

public class RobotCannon : Cannon
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "DropStuff",
            (p, i) =>
            {
                p.Translate(new Vector2(7f, 9f));
                p.DirectionMMF2 = 12;
                p.VelocityMMF2 = 10 + i;
                p.GravityMMF2 = 10 + random.Next(5);
            });
    }
}
