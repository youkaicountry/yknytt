using Godot;
using YKnyttLib;

public class GDKnyttArea : Node2D
{
    public GDAreaTiles Tiles { get; private set; }
    public GDObjectLayers Objects { get; private set; }
    public GDKnyttWorld GDWorld { get; private set; }
    public KnyttArea Area { get; private set; }

    bool active = false;
    PackedScene objects_scene;
    PackedScene tiles_scene;

    public GDKnyttArea()
    {
        objects_scene = ResourceLoader.Load("res://knytt/objects/ObjectLayers.tscn") as PackedScene;
        tiles_scene = ResourceLoader.Load("res://knytt/AreaTiles.tscn") as PackedScene;
    }

    public Vector2 GlobalCenter
    {
        get 
        { 
            var gp = GlobalPosition;
            return new Vector2(gp.x + (KnyttArea.AREA_WIDTH * GDKnyttAssetManager.TILE_WIDTH)/2f,
                               gp.y + (KnyttArea.AREA_HEIGHT * GDKnyttAssetManager.TILE_HEIGHT)/2f);
        }
    }

    public Vector2 getTileLocation(KnyttPoint point)
    {
        var gp = GlobalPosition;
        return new Vector2(gp.x + GDKnyttAssetManager.TILE_WIDTH*point.x + GDKnyttAssetManager.TILE_WIDTH/2f, 
                       gp.y + GDKnyttAssetManager.TILE_HEIGHT*point.y + GDKnyttAssetManager.TILE_HEIGHT/2f);
    }

    public KnyttPoint getPosition(Vector2 p)
    {
        var gp = GlobalPosition;
        return new KnyttPoint((int)((p.x - GlobalPosition.x) / ((float)GDKnyttAssetManager.TILE_WIDTH)),
                              (int)((p.y - GlobalPosition.y) / ((float)GDKnyttAssetManager.TILE_HEIGHT)));
    }

    public bool isIn(Vector2 global_pos)
    {
        var gp = GlobalPosition;
        return (global_pos.x > gp.x && global_pos.x < gp.x + GDKnyttAssetManager.TILE_WIDTH*KnyttArea.AREA_WIDTH &&
                global_pos.y > gp.y && global_pos.y < gp.y + GDKnyttAssetManager.TILE_HEIGHT*KnyttArea.AREA_HEIGHT);
    }

    public void loadArea(GDKnyttWorld world, KnyttArea area)
    {
        this.GDWorld = world;
        this.Area = area;

        this.Name = area.Position.ToString();

        this.Position = new Vector2(area.Position.x * KnyttArea.AREA_WIDTH * GDKnyttAssetManager.TILE_WIDTH, 
                                    area.Position.y * KnyttArea.AREA_HEIGHT * GDKnyttAssetManager.TILE_HEIGHT);

        // If it's an empty area, quit loading here
        if (area.Empty) { return; }

        // Setup gradient
        GetNode<GDKnyttBackground>("Background").initialize(world.AssetManager.getGradient(area.Background));

        // Initialize the Layers
        Tiles = tiles_scene.Instance() as GDAreaTiles;
        this.Tiles.initTiles(this);
        AddChild(Tiles);
    }

    public void activateArea()
    {
        GetNode<Timer>("DeactivateTimer").Stop();
        if (this.active || this.Area.Empty) { return; }
        this.createObjectLayers();
        this.active = true;
    }

    private void createObjectLayers()
    {
        Objects = objects_scene.Instance() as GDObjectLayers;
        Objects.initLayers(this);
        AddChild(Objects);
    }

    private void removeObjectLayers()
    {
        if (Objects != null)
        {
            this.Objects.returnObjects();
            this.Objects.QueueFree();
        }
    }

    // We don't want this to be async, because it can be cancelled
    // A whooole bunch of threads could queue up waiting for an event that never occurs if async
    public void deactivateArea(float delay = .5f)
    {
        var timer = GetNode<Timer>("DeactivateTimer");
        timer.WaitTime = delay;
        timer.Start();
    }

    public void regenerateArea()
    {
        if (this.Area.Empty) { return; }
        this.removeObjectLayers();
        this.createObjectLayers();
    }

    public void _on_DeactivateTimer_timeout()
    {
        
        this.active = false;
    }

    public void destroyArea()
    {
        if (Area.Empty) { return; }
        if (active && this.Objects != null) { Objects.returnObjects(); }
        GDWorld.AssetManager.returnTileSet(Area.TilesetA);
        GDWorld.AssetManager.returnTileSet(Area.TilesetB);
        GDWorld.AssetManager.returnGradient(Area.Background);
    }
}
