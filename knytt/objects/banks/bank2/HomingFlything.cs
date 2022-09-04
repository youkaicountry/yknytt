using Godot;

public class HomingFlything : BuzzFlyer
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "HomingBullet",
            (p, i) =>
            {
                (p as HomingBullet).globalJuni = Juni;
                p.Translate(new Vector2(12, 12));
                p.Velocity = 100 * Mathf.Sqrt2;
                p.Direction = Mathf.Pi / 4 + i * Mathf.Pi / 2;
            });
    }

    private void _on_ShootTimer_timeout_ext()
    {
        currentDirection = Vector2.Zero;
        GetNode<AudioStreamPlayer2D>("ShootPlayer").Play();
        for (int i = 0; i < 4; i++)
        {
            GDArea.Bullets.Emit(this, i);
        }
    }
}
