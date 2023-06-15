using Godot;
using System.Linq;

public class DropFlything : BuzzFlyer
{
    [Export] protected int[] directions = null;
    [Export] protected int bulletsCount = 0;

    private static readonly Vector2[] directionsOverride = Directions.Where(d => d != Vector2.Zero).ToArray();

    protected Timer bulletTimer;
    protected int counter;

    public override void _Ready()
    {
        base._Ready();
        bulletTimer = GetNode<Timer>("BulletTimer");
        GDArea.Selector.Register(this);
        GDArea.Bullets.RegisterEmitter(this, "DropStuff",
            (p, i) =>
            {
                p.Translate(new Vector2(12 + (i > 8 && i < 24 ? -6 : 6), 12 + (i < 16 ? -4 : 4)));
                p.DirectionMMF2 = i;
                p.VelocityMMF2 = 30 + random.Next(2);
                p.GravityMMF2 = 12;
            });
    }

    private void _on_ShootTimer_timeout_ext()
    {
        counter = 0;
        bulletTimer.Start();
        GetNode<AudioStreamPlayer2D>("ShootPlayer").Play(1.4f);
    }

    private void _on_BulletTimer_timeout()
    {
        foreach (int dir in directions)
        {
            if (GDArea.Selector.IsObjectSelected(this))
            {
                GDArea.Bullets.Emit(this, dir);
            }
        }
        if (++counter >= bulletsCount)
        {
            bulletTimer.Stop();
        }
    }

    protected override void changeDirection()
    {
        currentDirection = random.NextElement(directionsOverride);
    }
}
