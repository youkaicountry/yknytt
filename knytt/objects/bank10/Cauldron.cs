using Godot;

public class Cauldron : Bouncer
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "CauldronSpike",
            (p, i) =>
            {
                p.Translate(new Vector2(12, 5));
                p.VelocityMMF2 = -getSpeed() * 14;
                p.DirectionMMF2 = i == 0 ? 9 : 7;
                p.GravityMMF2 = 20;
                p.EnableRotation = true;
            });
    }

    protected float getSpeed() { return speed; }

    protected override void _on_Area2D_body_entered(object body)
    {
        base._on_Area2D_body_entered(body);
        if (body is Juni juni) { return; }

        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
        GDArea.Bullets.Emit(this, 0);
        GDArea.Bullets.Emit(this, 1);
    }
}
