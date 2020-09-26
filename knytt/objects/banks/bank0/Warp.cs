using Godot;

public class Warp : GDKnyttBaseObject
{
    public override void _Ready()
    {
        GDArea.Area.Warp.loadWarpData();
    }

    protected override void _impl_initialize() { }

    protected override void _impl_process(float delta) { }
}
