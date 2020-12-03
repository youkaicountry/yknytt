using Godot;
using YKnyttLib;

public class CreaturesInfo : ItemInfo
{
    public override void _Ready() {}
    
    public override void updateItem(JuniValues values)
    {
        GetNode<Label>("Label").Text = values.getCreaturesCount().ToString();
    }
}
