using System;
using Godot;
using static YKnyttLib.JuniValues;

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

    public void addItem(string scene, int item_id = -1)
    {
        foreach (ItemInfo child in GetNode<Node>("ItemContainer").GetChildren())
        {
            if (child.ItemID == item_id && item_id != -1) { return; }
        }
        MarginRight += 24;
        var item_node = ResourceLoader.Load<PackedScene>($"res://knytt/ui/info_panel/{scene}.tscn").Instance<ItemInfo>();
        item_node.ItemID = item_id;
        var item_container = GetNode<Container>("ItemContainer");
        item_container.AddChild(item_node);
    }

    public void checkCustomPowers()
    {
        var game = GetNodeOrNull<GDKnyttGame>("/root/GKnyttGame");
        var frames = ResourceLoader.Load<SpriteFrames>("res://knytt/objects/bank0/PowerItemFrames.tres");

        foreach (int power in Enum.GetValues(typeof(PowerNames)))
        {
            string icon_path = $"custom objects/powericon{power}.png";
            if (!game.GDWorld.KWorld.worldFileExists(icon_path)) { continue; }
            int power_fixed = power < (int)PowerNames.RedKey ? power : 
                              power > (int)PowerNames.RedKey ? power - 1 : (int)PowerNames.Map;
            var new_anim = $"{game.GDWorld.KWorld.WorldDirectoryName} {power_fixed} icon";
            if (frames.HasAnimation(new_anim)) { continue; }
            frames.AddAnimation(new_anim);
            frames.AddFrame(new_anim, game.GDWorld.KWorld.getWorldTexture(icon_path) as Texture);
        }

        foreach (ItemInfo child in GetNode<Node>("ItemContainer").GetChildren())
        {
            child.checkCustomIcon(game.GDWorld.KWorld.WorldDirectoryName);
        }

        string sheet = game.GDWorld.KWorld.INIData["World"]["Powers"];
        if (sheet == null || !game.GDWorld.KWorld.worldFileExists("custom objects/" + sheet)) { return; }
        var sheet_tex = game.GDWorld.KWorld.getWorldTexture("custom objects/" + sheet) as Texture;
        if (sheet_tex == null) { return; }

        for (int power = 0; power <= (int)PowerNames.RedKey; power++)
        {
            int power_fixed = power == (int)PowerNames.RedKey ? (int)PowerNames.Map : power;
            var new_anim = $"{game.GDWorld.KWorld.WorldDirectoryName} {power_fixed}";
            if (frames.HasAnimation(new_anim)) { continue; }
            frames.AddAnimation(new_anim);
            frames.SetAnimationSpeed(new_anim, 12);
            frames.SetAnimationLoop(new_anim, true);
            for (int i = 0; i < 8; i++)
            {
                frames.AddFrame(new_anim, new AtlasTexture() { Atlas = sheet_tex, Region = new Rect2(24 * i, 24 * power, 24, 24) });
            }
        }

        for (var power = PowerNames.RedKey; power <= PowerNames.PurpleKey; power++)
        {
            var new_anim = $"{game.GDWorld.KWorld.WorldDirectoryName} {(int)power}";
            if (frames.HasAnimation(new_anim)) { continue; }
            frames.AddAnimation(new_anim);
            frames.AddFrame(new_anim, new AtlasTexture() { Atlas = sheet_tex, Region = new Rect2(24 * (power - PowerNames.RedKey), 24 * 9, 24, 24) });
        }
    }
}
