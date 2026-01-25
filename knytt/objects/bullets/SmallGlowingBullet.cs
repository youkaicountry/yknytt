
public partial class SmallGlowingBullet : BaseBullet
{
    public override void _PhysicsProcess(double delta)
    {
        if (!Enabled) { return; }
        VelocityMMF2 += (float)delta * 20;
        base._PhysicsProcess(delta);
    }
}
