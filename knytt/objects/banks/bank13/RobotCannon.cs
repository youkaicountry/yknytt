using Godot;
using System;
using YUtil.Random;

public class RobotCannon : Cannon
{
    protected override void reinitializeBullet(BaseBullet p, int i)
    {
        p.Translate(new Vector2(8f, 8f));
        p.Direction = Mathf.Pi / 4;
        p.VelocityMMF2 = 10 + i;
        p.GravityMMF2 = 10 + GDKnyttDataStore.random.NextFloat(5);
    }
}
