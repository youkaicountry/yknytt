using Godot;

public class HomingRunner : WalkingShooter
{
    public override void _Ready()
    {
        GDArea.Bullets.RegisterEmitter(this, "HomingBullet",
            (p, i) => 
            {
                (p as HomingBullet).globalJuni = Juni;
                p.Translate(new Vector2(12, 8));
                p.Velocity = 100;
                p.Direction = Mathf.Pi / 2;
            });
        base._Ready();
    }
}
