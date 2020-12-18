using Godot;

public class StrangeRunner : WalkingShooter
{
    public override void _Ready()
    {
        GDArea.Bullets.RegisterEmitter(this, "RunnerBullet",
            (p, i) => 
            {
                p.Translate(new Vector2(12 + getDirection() * 6, 5));
                p.VelocityMMF2 = 20 + Mathf.Abs(Center.x - Juni.ApparentPosition.x) / 5 
                                    + Mathf.Abs(Center.y - Juni.ApparentPosition.y) / 4;
                p.DirectionMMF2 = 8 - getDirection() * (3 + random.Next(3));
                p.GravityMMF2 = 18;
            });
        base._Ready();
    }
}
