public class HomingBullet : BaseBullet
{
    public Juni globalJuni { protected get; set; }

    public override void _PhysicsProcess(float delta)
    {
        if (!Enabled) { return; }
        _velocity_x += 2.5f * (globalJuni.ApparentPosition.x < GlobalPosition.x ? -1 : 1);
        _velocity_y += 2.5f * (globalJuni.ApparentPosition.y < GlobalPosition.y ? -1 : 1);
        base._PhysicsProcess(delta);
    }
}
