using Godot;
using YKnyttLib;

public partial class CoinsInfo : CollectionInfo
{
    public override string IconFilename { get; } = "Custom Objects/CoinIcon.png";

    public override void updateItem(JuniValues values)
    {
        GetNode<Label>("Label").Text = values.getCoinCount().ToString();
    }
}
