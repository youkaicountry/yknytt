
public class SmallGlowingBullet : BaseBullet
{
    public override void _PhysicsProcess(float delta)
    {
        if (!Enabled) { return; }
        VelocityMMF2 += delta * 20;
        base._PhysicsProcess(delta);
    }
}
