using YKnyttLib;

public class Warp : GDKnyttBaseObject
{
    public override void _Ready()
    {
        if (GDArea.Area.ExtraData == null) { return; }
        GDArea.Area.Warp.loadWarpData();
    }

    public override void customDeletion()
    {
        GDArea.Area.Warp = new KnyttWarp(GDArea.Area);
    }
}
