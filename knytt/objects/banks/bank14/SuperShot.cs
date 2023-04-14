using Godot;

public partial class SuperShot : GesturesObject
{
    [Export] int[] shotDirections = null;
    [Export] Vector2 shotPosition = Vector2.Zero;

    public override void _Ready()
    {
        base._Ready();

        GDArea.Bullets.RegisterEmitter(this, "SuperBullet",
            (p, i) =>
            {
                p.Translate(shotPosition);
                p.VelocityMMF2 = 4 + random.Next(8); //5 + random.Next(6); -- original formula
                p.DirectionMMF2 = random.NextElement(shotDirections);
                p.DecelerationMMF2 = 3;
            });

        _on_PrepareTimer_timeout();
    }

    private void _on_LoopTimer_timeout_ext()
    {
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
        for (int i = 0; i < 25; i++)
        {
            GDArea.Bullets.Emit(this, i);
        }

        nextAnimation();
        GetNode<Timer>("PrepareTimer").Start();
    }

    private void _on_PrepareTimer_timeout()
    {
        timer.Stop();
        sprite.Play("_launch");
    }
}
