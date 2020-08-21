using Godot;
using YKnyttLib;

public class GDKnyttArea : Node2D
{
    public GDAreaTiles Tiles { get; private set; }
    public GDKnyttWorld World { get; private set; }
    public KnyttArea<string> Area { get; private set; }

    public Vector2 GlobalCenter
    {
        get 
        { 
            var gp = GlobalPosition;
            return new Vector2(GlobalPosition.x + (KnyttArea<string>.AREA_WIDTH * GDKnyttAssetBuilder.TILE_WIDTH)/2f,
                               GlobalPosition.y + (KnyttArea<string>.AREA_HEIGHT * GDKnyttAssetBuilder.TILE_HEIGHT)/2f);
        }
    }

    public void loadArea(GDKnyttWorld world, KnyttArea<string> area)
    {
        this.World = world;
        this.Area = area;

        this.Name = area.Position.ToString();

        this.Tiles = this.GetNode("AreaTiles") as GDAreaTiles;

        this.Position = new Vector2(area.Position.x * KnyttArea<string>.AREA_WIDTH * GDKnyttAssetBuilder.TILE_WIDTH, 
                                    area.Position.y * KnyttArea<string>.AREA_HEIGHT * GDKnyttAssetBuilder.TILE_HEIGHT);

        // If it's an empty area, quit loading here
        if (area.Empty) { return; }

        TileSet ta = world.AssetManager.getTileSet(area.TilesetA);
        TileSet tb = world.AssetManager.getTileSet(area.TilesetB);

        // Setup background gradient
        ((GDKnyttBackground)GetNode("Background")).initialize(world.AssetManager.getGradient(area.Background));

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

    public void destroyArea()
    {
        if (Area.Empty) { return; }
        World.AssetManager.returnTileSet(Area.TilesetA);
        World.AssetManager.returnTileSet(Area.TilesetB);
        World.AssetManager.returnGradient(Area.Background);
    }
}
