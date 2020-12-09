using Godot;

public class BlueBullet : BaseBullet
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "BlueBulletExplosion",
            (p, i) => 
            {
                p.Modulate = new Color(2, 2, 2, 0.75f);
                p.DirectionMMF2 = i;
                p.VelocityMMF2 = 25;
                p.DecelerationMMF2 = 15;
            });
    }

    protected override void disappear(bool collide)
    {
        if (collide) { GDArea.Bullets.EmitMany(this, 32); }
        base.disappear(collide);
    }
}
