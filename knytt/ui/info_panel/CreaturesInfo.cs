using Godot;
using YKnyttLib;

public partial class CreaturesInfo : CollectionInfo
{
    public override string IconFilename { get; } = "Custom Objects/CreatureIcon.png";

    public override void updateItem(JuniValues values)
    {
        GetNode<Label>("Label").Text = values.getCreaturesCount().ToString();
    }
}
