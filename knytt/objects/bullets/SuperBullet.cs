using Godot;

public partial class SuperBullet : BaseBullet
{
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Translate(new Vector2(GDKnyttDataStore.random.Next(3) - 1, GDKnyttDataStore.random.Next(3) - 1) * (float)delta * 30);
    }

    private void onDisappear(int limit)
    {
        if (!Enabled) { return; }
        // TODO: O(n^2) check, maybe replace with ObjectSelector Register/Unregister/GetSize?
        int count = GDArea.Bullets.GetBulletsCount("SuperBullet");
        if (count > limit && GDKnyttDataStore.random.Next(count) == 0)
        {
            disappear(true);
        }
    }
}
