using Godot;
using System;
using YUtil.Random;

public class HomingCannon : Cannon
{
    protected override void reinitializeBullet(BaseBullet p, int i)
    {
        p.Translate(new Vector2(8f, 8f));
        p.DirectionMMF2 = 4;
        p.VelocityMMF2 = Mathf.Max(0, Center.x - Juni.ApparentPosition.x) / 8f + 13f + random.NextFloat(6);
        p.GravityMMF2 = 10 + random.NextFloat(5);
    }

    protected override void playSound()
    {
        player.Play(1f);
    }
}
