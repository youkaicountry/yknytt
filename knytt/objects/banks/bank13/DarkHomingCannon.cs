using Godot;
using YUtil.Random;

public class DarkHomingCannon : Cannon
{
    protected override void reinitializeBullet(BaseBullet p, int i)
    {
        p.Translate(new Vector2(8f, 8f));
        p.Direction = Mathf.Pi / 4;
        p.VelocityMMF2 = -((Juni.ApparentPosition.x - GlobalPosition.x) / 8f - 13f - GDKnyttDataStore.random.NextFloat(6));
        p.GravityMMF2 = 20 + GDKnyttDataStore.random.NextFloat(3);
    }

    protected override void playSound()
    {
        player.Play(1.55f);
    }
}
