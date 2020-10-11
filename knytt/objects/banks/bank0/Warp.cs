using Godot;

public class Warp : GDKnyttBaseObject
{
    public override void _Ready()
    {
        GDArea.Area.Warp.loadWarpData();
    }
}
