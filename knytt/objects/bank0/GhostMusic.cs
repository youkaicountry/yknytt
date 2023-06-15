using YKnyttLib;
using Godot;

public class GhostMusic : GDKnyttBaseObject
{
    public override void _Ready()
    {
        GDArea.PlayNoMusic = !Juni.Powers.getPower(JuniValues.PowerNames.Eye);
    }
}
