using Godot;

public class BigGlowingBullet : BaseBullet
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "SmallGlowingBullet",
            (p, i) =>
            {
                var sign = Direction < Mathf.Pi ? -1 : 1;
                p.Translate(new Vector2(0, -sign * 2));
                p.DirectionMMF2 = sign * i;
                p.VelocityMMF2 = 5;
                p.DisappearWhenStopped = true;
            });
    }

    public override void disappear(bool collide)
    {
        if (collide) { GDArea.Bullets.EmitMany(this, 17); }
        base.disappear(collide);
    }
}
