using Godot;

public class SuperBullet : BaseBullet
{
    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        Translate(new Vector2(GDKnyttDataStore.random.Next(3) - 1, GDKnyttDataStore.random.Next(3) - 1) * delta * 30);
    }

    private void onDisappear(int limit)
    {
        if (!Enabled) { return; }
        int count = GDArea.Selector.GetSize(this, by_type: true);
        if (count > limit && GDKnyttDataStore.random.Next(count) == 0)
        {
            disappear(true);
        }
    }

    public override bool Enabled
    {
        set
        {
            if (value) { GDArea.Selector.Register(this, by_type: true); }
            else { GDArea.Selector.Unregister(this, by_type: true); }
            base.Enabled = value;
        }
    }
}
