using Godot;
using YKnyttLib;

public class GKnyttArea : Node2D
{
    public AreaTiles Tiles { get; private set; }
    public GKnyttWorld World { get; private set; }
    public KnyttArea<string> Area { get; private set; }

    public Vector2 GlobalCenter
    {
        get 
        { 
            var gp = GlobalPosition;
            return new Vector2(GlobalPosition.x + (KnyttArea<string>.AREA_WIDTH * GKnyttAssetBuilder.TILE_WIDTH)/2f,
                               GlobalPosition.y + (KnyttArea<string>.AREA_HEIGHT * GKnyttAssetBuilder.TILE_HEIGHT)/2f);
        }
    }

    public void loadArea(GKnyttWorld world, KnyttArea<string> area)
    {
        this.World = world;
        this.Area = area;

        this.Tiles = this.GetNode("AreaTiles") as AreaTiles;

        this.Position = new Vector2(area.Position.x * KnyttArea<string>.AREA_WIDTH * GKnyttAssetBuilder.TILE_WIDTH, 
                                    area.Position.y * KnyttArea<string>.AREA_HEIGHT * GKnyttAssetBuilder.TILE_HEIGHT);

        // If it's an empty area, quit loading here
        if (area.Empty) { return; }

        TileSet ta = world.GetTileSet(area.TilesetA);
        TileSet tb = world.GetTileSet(area.TilesetB);

        // Setup background gradient
        ((GKnyttBackground)GetNode("Background")).initialize(world.getGradient(area.Background));

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
