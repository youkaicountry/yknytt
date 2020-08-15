using Godot;
using YKnyttLib;

public class GKnyttArea : Node2D
{
    public AreaTiles Tiles { get; private set; }

    public override void _Ready()
    {
        
    }

    public void loadArea(GKnyttWorld world, KnyttArea<string> area)
    {
        this.Tiles = this.GetNode("AreaTiles") as AreaTiles;

        TileSet ta = world.GetTileSet(area.TilesetA);
        TileSet tb = world.GetTileSet(area.TilesetB);

        // Initialize the Layers
        this.Tiles.initTiles(ta, tb);

        // Draw the map
        for (int layer = 0; layer < KnyttArea<string>.AREA_TILE_LAYERS; layer++ )
        {
            var data = area.TileLayers[layer];
            for (int y = 0; y < KnyttArea<string>.AREA_HEIGHT; y++)
            {
                for (int x = 0; x < KnyttArea<string>.AREA_WIDTH; x++)
                {
                    this.Tiles.setTile(layer, x, y, data.getTile(x, y));
                }
            }
        }
    }
}
