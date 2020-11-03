using Godot;
using System;

public class BigGlowingBullet : BaseBullet
{
    public override void _Ready()
    {
        base._Ready();
        var bulletScene = ResourceLoader.Load<PackedScene>("res://knytt/objects/bullets/SmallGlowingBullet.tscn");
        GDArea.Bullets.RegisterEmitter(this, 50,
            () => bulletScene.Instance() as BaseBullet,
            (p, i) => 
            {
                var sign = Direction < Mathf.Pi ? -1 : 1;
                p.Translate(new Vector2(0, -sign * 2));
                p.Direction = sign * Mathf.Pi * i / 16;
                p.VelocityMMF2 = 10;
            });
    }

    protected override void disappear(bool collide)
    {
        if (collide) { GDArea.Bullets.EmitMany(this, 17); }
        base.disappear(collide);
    }
}
