public partial class FireBullet2 : BaseBullet
{
    public override void _PhysicsProcess(double delta)
    {
        if (!Enabled) { return; }
        Direction += (GDKnyttDataStore.random.Next(3) - 1) * (float)delta * 0.9f;
        base._PhysicsProcess(delta);
    }
}
