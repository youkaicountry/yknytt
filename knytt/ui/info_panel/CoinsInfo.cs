using Godot;
using YKnyttLib;

public class CoinsInfo : ItemInfo
{
    public override void _Ready() {}
    
    public override void updateItem(JuniValues values)
    {
        GetNode<Label>("Label").Text = values.getCoinCount().ToString();
    }
}
