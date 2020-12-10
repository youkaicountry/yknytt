using Godot;

public class RobotCannon : Cannon
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "DropStuff", 
            (p, i) =>
            {
                p.Translate(new Vector2(8f, 8f));
                p.DirectionMMF2 = 4;
                p.VelocityMMF2 = 10 + i;
                p.GravityMMF2 = 10 + random.Next(5);
            });
    }
}
