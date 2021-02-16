using Godot;

public class Bee : Muff
{
    [Export] protected int chance = 0;
    [Export] protected Color bulletModulate = default(Color);
    [Export] protected float shootTimer = 0;

    public override void _Ready()
    {
        base._Ready();
        GetNode<Timer>("ShootTimer").WaitTime = shootTimer;
        GDArea.Bullets.RegisterEmitter(this, "BeeBullet",
            (p, i) =>
            {
                p.Translate(new Vector2(12 + (i > 0 ? 5 : -5), 24));
                p.Modulate = bulletModulate;
                p.GetNode<AnimatedSprite>("AnimatedSprite").FlipH = i > 0;
                p.VelocityMMF2 = 30;
                p.DirectionMMF2 = (i < 0 ? 19 : 27) + random.Next(3);
            });
    }

    private void _on_ShootTimer_timeout()
    {
        if (random.Next(chance) != 0) { return; }
        GetNode<RawAudioPlayer2D>("ShotPlayer").Play();
        GDArea.Bullets.Emit(this, direction < 0 ? -1 : 1);
    }
}
