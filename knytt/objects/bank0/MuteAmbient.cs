public class MuteAmbient : GDKnyttBaseObject
{
    public override void _Ready()
    {
        if (ObjectID.y == 39) { GDArea.Ambiance1CustomVolume = true; } else { GDArea.Ambiance2CustomVolume = true; }
    }
}
