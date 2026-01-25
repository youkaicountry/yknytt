public partial class MuteAmbient : GDKnyttBaseObject
{
    public override void _Ready()
    {
        if (ObjectID.Y == 39) { GDArea.Ambiance1CustomVolume = true; } else { GDArea.Ambiance2CustomVolume = true; }
    }
}
