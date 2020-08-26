using Godot;
using YKnyttLib;

public class GDKnyttArea : Node2D
{
    public GDAreaTiles Tiles { get; private set; }
    public GDKnyttWorld GDWorld { get; private set; }
    public KnyttArea Area { get; private set; }

    public Vector2 GlobalCenter
    {
        get 
        { 
            var gp = GlobalPosition;
            return new Vector2(GlobalPosition.x + (KnyttArea.AREA_WIDTH * GDKnyttAssetManager.TILE_WIDTH)/2f,
                               GlobalPosition.y + (KnyttArea.AREA_HEIGHT * GDKnyttAssetManager.TILE_HEIGHT)/2f);
        }
    }

    public Vector2 getTileLocation(KnyttPoint point)
    {
        return new Vector2(GlobalPosition.x + GDKnyttAssetManager.TILE_WIDTH*point.x + GDKnyttAssetManager.TILE_WIDTH/2f, 
                       GlobalPosition.y + GDKnyttAssetManager.TILE_HEIGHT*point.y + GDKnyttAssetManager.TILE_HEIGHT/2f);
    }

    public void loadArea(GDKnyttWorld world, KnyttArea area)
    {
        this.GDWorld = world;
        this.Area = area;

        this.Name = area.Position.ToString();

        this.Tiles = this.GetNode("AreaTiles") as GDAreaTiles;

        this.Position = new Vector2(area.Position.x * KnyttArea.AREA_WIDTH * GDKnyttAssetManager.TILE_WIDTH, 
                                    area.Position.y * KnyttArea.AREA_HEIGHT * GDKnyttAssetManager.TILE_HEIGHT);

        // If it's an empty area, quit loading here
        if (area.Empty) { return; }

        TileSet ta = world.AssetManager.getTileSet(area.TilesetA);
        TileSet tb = world.AssetManager.getTileSet(area.TilesetB);

        // Setup background gradient
        ((GDKnyttBackground)GetNode("Background")).initialize(world.AssetManager.getGradient(area.Background));

        // Initialize the Layers
        this.Tiles.initTiles(ta, tb);

        // Draw the map
        for (int layer = 0; layer < KnyttArea.AREA_TILE_LAYERS; layer++ )
        {
            var data = area.TileLayers[layer];
            for (int y = 0; y < KnyttArea.AREA_HEIGHT; y++)
            {
                for (int x = 0; x < KnyttArea.AREA_WIDTH; x++)
                {
                    var tile = data.getTile(x, y);
                    if (tile == 0 || tile == 128) { continue; }
                    this.Tiles.setTile(layer, x, y, data.getTile(x, y));
                }
            }
        }
    }

    public void destroyArea()
    {
        if (Area.Empty) { return; }
        GDWorld.AssetManager.returnTileSet(Area.TilesetA);
        GDWorld.AssetManager.returnTileSet(Area.TilesetB);
        GDWorld.AssetManager.returnGradient(Area.Background);
    }
}
