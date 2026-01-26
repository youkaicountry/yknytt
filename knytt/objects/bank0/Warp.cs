using YKnyttLib;

public partial class Warp : GDKnyttBaseObject
{
    public override void _Ready()
    {
        if (GDArea.Area3D.ExtraData == null) { return; }
        GDArea.Area3D.Warp.loadWarpData();
    }

    public override void customDeletion()
    {
        GDArea.Area3D.Warp = new KnyttWarp(GDArea.Area3D);
    }
}
