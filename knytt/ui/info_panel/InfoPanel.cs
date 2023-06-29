using Godot;

public class InfoPanel : Panel
{
    bool hasCoins;
    bool hasCreatures;
    bool hasArtifacts;

    public void updateItems(Juni juni)
    {
        if (!hasCreatures && juni.Powers.getCreaturesCount() > 0) { addItem("CreaturesInfo"); hasCreatures = true; }
        if (!hasCoins && juni.Powers.getCoinCount() > 0) { addItem("CoinsInfo"); hasCoins = true; }
        if (!hasArtifacts && juni.Powers.getArtifactsCount() > 0) { addItem("ArtifactsInfo"); hasArtifacts = true; }

        foreach (ItemInfo child in GetNode<Node>("ItemContainer").GetChildren())
        {
            child.updateItem(juni.Powers);
        }
    }

    public void addItem(string scene, int item_id = 0)
    {
        foreach (ItemInfo child in GetNode<Node>("ItemContainer").GetChildren())
        {
            if (child.ItemID == item_id && item_id != 0) { return; }
        }
        MarginRight += 24;
        var item_node = ResourceLoader.Load<PackedScene>($"res://knytt/ui/info_panel/{scene}.tscn").Instance() as ItemInfo;
        item_node.ItemID = item_id;
        var item_container = GetNode<Container>("ItemContainer");
        item_container.AddChild(item_node);
    }
}
