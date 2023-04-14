public partial class MuteAmbient : GDKnyttBaseObject
{
    public override void _Ready()
    {
        // TODO: what's the purpose of this object? Why don't just set ambient track to 0?
        if (ObjectID.y == 39) { GDArea.PlayNoAmbiance1 = true; } else { GDArea.PlayNoAmbiance2 = true; }
    }
}
