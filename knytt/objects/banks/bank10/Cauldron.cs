using Godot;

public class Cauldron : Bouncer
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "CauldronSpike",
            (p, i) => 
            {
                p.Translate(new Vector2(12 + (i == 0 ? -7 : 7), 5));
                p.VelocityMMF2 = -getSpeed() * 14;
                p.DirectionMMF2 = i == 0 ? 7 : 9;
                p.GravityMMF2 = 20;
                p.EnableRotation = true;
            });
    }

    protected float getSpeed() { return speed; }

    protected override void _on_Area2D_body_entered(object body)
    {
        base._on_Area2D_body_entered(body);
        if (body is Juni juni) { return; }

        GetNode<RawAudioPlayer2D>("ShotPlayer").Play();
        GDArea.Bullets.Emit(this, 0);
        GDArea.Bullets.Emit(this, 1);
    }
}
