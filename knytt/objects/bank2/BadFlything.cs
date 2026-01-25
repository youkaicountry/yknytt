using Godot;

public partial class BadFlything : BuzzFlyer
{
    [Export] public int[] directions = null;

    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "FlythingBullet",
            (p, i) =>
            {
                p.Translate(new Vector2(12, 12));
                p.VelocityMMF2 = 30;
                p.DirectionMMF2 = i;
                p.EnableRotation = true;
            });
    }

    private void _on_ShootTimer_timeout_ext()
    {
        currentDirection = Vector2.Zero;
        GetNode<AudioStreamPlayer2D>("ShootPlayer").Play();
        foreach (int dir in directions)
        {
            GDArea.Bullets.Emit(this, dir);
        }
    }
}
