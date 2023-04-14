using Godot;
using System;

public partial class ArtifactsPanel : Control
{
    private static string COIN_ICON_PATH = "Custom Objects/CoinIcon.png";
    public override void _Ready()
    {
        var world = (FindParent("GKnyttGame") as GDKnyttGame).GDWorld.KWorld;
        
        if (world.worldFileExists(COIN_ICON_PATH))
        {
            GetNode<TextureRect>("CoinControl/CoinIcon").Texture = world.getWorldTexture(COIN_ICON_PATH) as Texture2D;
        }

        for (int i = 1; i <= 7; i++)
        {
            if (!world.INIData["World"].ContainsKey($"Artifact{i}")) { continue; }
            var image_path = "Custom Objects/" + world.INIData["World"][$"Artifact{i}"];
            if (!world.worldFileExists(image_path)) { continue; }
            var texture = GetNode<TextureRect>($"ArtifactsControl/Artifact{i}Control/ArtifactIcon").Texture as AtlasTexture;
            texture.Atlas = world.getWorldTexture(image_path) as Texture2D;
        }
    }

    public void updateItems(Juni juni)
    {
        GetNode<Control>("CreaturesControl").Visible = juni.Powers.getCreaturesCount() > 0;
        for (int i = 1; i <= 50; i++)
        {
            var mat = GetNode<TextureRect>($"CreaturesControl/Creature{i}").Material as ShaderMaterial;
            mat.SetShaderParameter("grayscale", !juni.Powers.Collectables[i]);
        }

        GetNode<Control>("CoinControl").Visible = juni.Powers.getCoinCount() + juni.Powers.CoinsSpent > 0;
        Vector2 coinFullSize = GetNode<ColorRect>("CoinControl/FullCoinProgress").Size;
        float progressWidth = coinFullSize.X * juni.Powers.getCoinCount() / 100;
        GetNode<ColorRect>("CoinControl/CurrentProgress").Size = new Vector2(progressWidth, coinFullSize.Y);
        
        var spentProgressRect = GetNode<ColorRect>("CoinControl/SpentProgress");
        spentProgressRect.Size = new Vector2(coinFullSize.X * juni.Powers.CoinsSpent / 100, coinFullSize.Y);
        spentProgressRect.Position = new Vector2(progressWidth, 0);

        for (int i = 0; i < 7; i++)
        {
            GetNode<Control>($"ArtifactsControl/Artifact{i+1}Control").Visible = juni.Powers.getArtifactsCount(i) > 0;
            Vector2 artifactFullSize = GetNode<ColorRect>($"ArtifactsControl/Artifact{i+1}Control/FullProgress").Size;
            GetNode<ColorRect>($"ArtifactsControl/Artifact{i+1}Control/CurrentProgress").Size = 
                new Vector2(artifactFullSize.X * juni.Powers.getArtifactsCount(i) / 7, coinFullSize.Y);
        }
    }
}
