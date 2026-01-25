public partial class HomingBullet : BaseBullet
{
    public Juni globalJuni { protected get; set; }

    public override void _PhysicsProcess(double delta)
    {
        if (!Enabled) { return; }
        VelocityX += 2.5f * (globalJuni.ApparentPosition.X < GlobalPosition.X ? -1 : 1);
        VelocityY += 2.5f * (globalJuni.ApparentPosition.Y < GlobalPosition.Y ? -1 : 1);
        base._PhysicsProcess(delta);
    }
}
