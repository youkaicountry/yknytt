public class Spikes : GDKnyttBaseObject
{
    public override void _Ready()
    {
        GetNode<DistanceMod>("SpikerMod").globalJuni = Juni;
    }
}
