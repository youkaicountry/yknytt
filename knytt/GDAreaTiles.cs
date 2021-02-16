using Godot;
using System.Collections.Generic;
using YKnyttLib;

public class GDAreaTiles : Node2D
{
    public GDAreaLayer[] Layers { get; private set; }

    public void initTiles(GDKnyttArea area)
    {
        var llayers = new List<GDAreaLayer>();

        TileSet ta = area.GDWorld.AssetManager.getTileSet(area.Area.TilesetA);
        TileSet tb = area.GDWorld.AssetManager.getTileSet(area.Area.TilesetB);

        foreach (GDAreaLayer c in this.GetChildren())
        {
            llayers.Add(c);
            c.initLayer(ta, tb);
        }

        // Render map
        this.Layers = llayers.ToArray();

        for (int layer = 0; layer < KnyttArea.AREA_TILE_LAYERS; layer++)
        {
            var data = area.Area.TileLayers[layer];
            for (int y = 0; y < KnyttArea.AREA_HEIGHT; y++)
            {
                for (int x = 0; x < KnyttArea.AREA_WIDTH; x++)
                {
                    var tile = data.getTile(x, y);
                    if (tile == 0 || tile == 128) { continue; }
                    setTile(layer, x, y, data.getTile(x, y));
                }
            }
        }

        if (area.Area.getExtraData("Overlay")?.ToLower() == "true") { Layers[2].ZIndex = 12; }
    }

    public void setTile(int layer, int x, int y, int tilenum)
    {
        this.Layers[layer].setTile(x, y, tilenum);
    }

    public void deactivate()
    {
        foreach (var layer in Layers)
        {
            layer.deactivate();
        }
    }

    public void activate()
    {
        foreach (var layer in Layers)
        {
            layer.activate();
        }
    }
}
