public class BlockUser : GDKnyttBaseObject
{
    public override void _Ready()
    {
        GDArea.BlockInput = true;
    }

    public override void customDeletion()
    {
        GDArea.BlockInput = false;
    }
}
