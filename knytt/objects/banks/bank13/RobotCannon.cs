using Godot;
using YUtil.Random;

public class RobotCannon : Cannon
{
    protected override void reinitializeBullet(BaseBullet p, int i)
    {
        p.Translate(new Vector2(8f, 8f));
        p.DirectionMMF2 = 4;
        p.VelocityMMF2 = 10 + i;
        p.GravityMMF2 = 10 + random.NextFloat(5);
    }
}
