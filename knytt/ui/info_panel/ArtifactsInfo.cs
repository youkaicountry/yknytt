using Godot;
using YKnyttLib;

public class ArtifactsInfo : ItemInfo
{
    public override void _Ready() {}

    public override void updateItem(JuniValues values)
    {
        GetNode<Label>("Label").Text = values.getArtifactsCount().ToString();
    }
}
