using Godot;

public class InfoPanel : Panel
{
    public void updateItems(Juni juni)
    {
        foreach (var child in GetNode<Node>("ItemContainer").GetChildren())
        {
            ItemInfo ii = child as ItemInfo;
            ii.updateItem(juni.Powers);
        }
    }
}
