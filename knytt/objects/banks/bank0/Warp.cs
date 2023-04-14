public partial class Warp : GDKnyttBaseObject
{
    public override void _Ready()
    {
        if (GDArea.Area.ExtraData == null) { return; }
        GDArea.Area.Warp.loadWarpData();
    }
}
