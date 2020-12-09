using Godot;
using YUtil.Random;

public class DarkHomingCannon : Cannon
{
    // TODO: first delay

    protected override void reinitializeBullet(BaseBullet p, int i)
    {
        p.Translate(new Vector2(8f, 8f));
        p.DirectionMMF2 = 4;
        p.VelocityMMF2 = Mathf.Max(0, Center.x - Juni.ApparentPosition.x) / 10f + 36f + random.NextFloat(3);
        p.GravityMMF2 = 20 + random.NextFloat(3);
    }

    protected override void playSound()
    {
        player.Play(1.55f);
    }
}
