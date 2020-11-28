public class FireBullet2 : BaseBullet
{
    public override void _PhysicsProcess(float delta)
    {
        if (!Enabled) { return; }
        Direction += (GDKnyttDataStore.random.Next(3) - 1) * delta * 0.9f;
        base._PhysicsProcess(delta);
    }
}
