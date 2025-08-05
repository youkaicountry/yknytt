public class SwimScreen : GDKnyttBaseObject
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Swim = true;
    }

    public override void customDeletion()
    {
        GDArea.Swim = false;
    }
}
