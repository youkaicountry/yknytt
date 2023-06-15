public class MachinePart4 : GDKnyttBaseObject
{
    public override void _Ready()
    {
        Set("frame", random.Next(8));
    }
}
