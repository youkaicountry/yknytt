using YKnyttLib;

public class Warp : GDKnyttBaseObject
{
    public override void _Ready()
    {
        if (GDArea.Area.ExtraData == null) { return; }
        GDArea.Area.Warp.loadWarpData();
    }

    private void _on_Warp_tree_exiting()
    {
        GDArea.Area.Warp = new KnyttWarp(GDArea.Area);
    }
}
