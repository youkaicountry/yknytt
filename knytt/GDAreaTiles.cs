using Godot;
using YKnyttLib;

public partial class GDAreaTiles : Node2D
{
    public TileMap tileMapA;
    public TileMap tileMapB;

    public void initTiles(GDKnyttArea area)
    {
        tileMapA = GetNode<TileMap>("TileMapA");
        tileMapB = GetNode<TileMap>("TileMapB");

        tileMapA.TileSet = area.GDWorld.AssetManager.getTileSet(area.Area.TilesetA);
        tileMapB.TileSet = area.GDWorld.AssetManager.getTileSet(area.Area.TilesetB);

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

        if (area.Area.getExtraData("Overlay")?.ToLower() == "true")
        {
            tileMapA.SetLayerZIndex(2, 12);
            tileMapB.SetLayerZIndex(2, 12);
        }
    }

    public void setTile(int layer, int x, int y, int tilenum)
    {
        TileMap tm;
        if (tilenum < 128)
        {
            tm = tileMapB;
        }
        else
        {
            tilenum -= 128;
            tm = tileMapA;
        }

        tm.SetCell(layer, new Vector2I(x, y), layer == 3 ? 1 : 0, 
            new Vector2I(tilenum % GDKnyttAssetManager.TILESET_WIDTH, tilenum / GDKnyttAssetManager.TILESET_WIDTH));
    }

    public void deactivate()
    {
        for (int i = 0; i < tileMapA.GetLayersCount(); i++)
        {
            tileMapA.SetLayerEnabled(i, false);
            tileMapB.SetLayerEnabled(i, false);
        }
    }

    public void activate()
    {
        for (int i = 0; i < tileMapA.GetLayersCount(); i++)
        {
            tileMapA.SetLayerEnabled(i, true);
            tileMapB.SetLayerEnabled(i, true);
        }
    }
}
