using Godot;

public class Ninja : WalkingShooter
{
    public override void _Ready()
    {
        GDArea.Bullets.RegisterEmitter(this, "NinjaStar",
            (p, i) => 
            {
                p.Translate(new Vector2(12 + getDirection() * 5, 17));
                p.VelocityMMF2 = 80;
                p.DirectionMMF2 = 8 - getDirection() * (4 + random.Next(3));
                p.GravityMMF2 = 18;
            });
        base._Ready();
    }
}
