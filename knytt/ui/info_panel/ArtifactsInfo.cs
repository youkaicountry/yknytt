using Godot;
using YKnyttLib;

public partial class ArtifactsInfo : CollectionInfo
{
    public override string IconFilename { get; } = "Custom Objects/ArtifactIcon.png";

    public override void updateItem(JuniValues values)
    {
        GetNode<Label>("Label").Text = values.getArtifactsCount().ToString();
    }
}
