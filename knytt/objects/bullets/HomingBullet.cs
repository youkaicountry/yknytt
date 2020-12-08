public class HomingBullet : BaseBullet
{
    public Juni globalJuni { protected get; set; }

    public override void _PhysicsProcess(float delta)
    {
        if (!Enabled) { return; }
        VelocityX += 2.5f * (globalJuni.ApparentPosition.x < GlobalPosition.x ? -1 : 1);
        VelocityY += 2.5f * (globalJuni.ApparentPosition.y < GlobalPosition.y ? -1 : 1);
        base._PhysicsProcess(delta);
    }
}
