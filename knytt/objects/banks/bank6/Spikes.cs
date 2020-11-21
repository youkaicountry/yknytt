public class Spikes : GDKnyttBaseObject
{
    public override void _Ready()
    {
        GetNode<SpikerMod>("SpikerMod").globalJuni = Juni;
    }
}
