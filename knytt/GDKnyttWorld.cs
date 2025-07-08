using Godot;
using YKnyttLib;
using YKnyttLib.Paging;

public class GDKnyttWorld : Node2D
{
    PackedScene area_scene;

    public string MapPath { get; internal set; }
    public string WorldConfigPath { get; internal set; }
    public KnyttRectPaging<GDKnyttArea> Areas { get; }
    public GDKnyttAssetManager AssetManager { get; }
    public GDKnyttGame Game { get; protected set; }

    public GDKnyttWorldImpl KWorld { get; private set; }

    [Export]
    public int worldID = 0;

    public GDKnyttWorld()
    {
        this.AssetManager = new GDKnyttAssetManager(this, tile_cache: 32, gradient_cache: 16, song_cache: 4, ambiance_cache: 8, object_cache: 64);
        var border_size = GDKnyttSettings.ScrollType == GDKnyttSettings.ScrollTypes.Original ? new KnyttPoint(0, 0) :
                          GDKnyttSettings.ScrollType == GDKnyttSettings.ScrollTypes.Smooth ? new KnyttPoint(1, 1) :
                          GDKnyttSettings.SeamlessScroll ? new KnyttPoint(1, 0) : new KnyttPoint(0, 0);
        this.Areas = new KnyttRectPaging<GDKnyttArea>(border_size);
        this.Areas.OnPageIn = (KnyttPoint loc) => instantiateArea(loc);
        this.Areas.OnPageOut = (KnyttPoint loc, GDKnyttArea area) => area?.destroyArea();

        this.area_scene = ResourceLoader.Load("res://knytt/GDKnyttArea.tscn") as PackedScene;
    }

    public void setWorld(GDKnyttGame game, GDKnyttWorldImpl world)
    {
        this.KWorld = world;
        this.Game = game;
    }

    public void loadWorld()
    {
        // If info is not initialized, load it
        if (KWorld.Info == null)
        {
            var txt = GDKnyttAssetManager.loadTextFileRaw(KWorld.getWorldData("World.ini"));
            KWorld.loadWorldConfig(txt);
        }

        var map_data = KWorld.getWorldData("Map.bin");
        System.IO.MemoryStream map_stream = new System.IO.MemoryStream(map_data);

        this.KWorld.loadWorldMap(map_stream);
    }

    public GDKnyttArea getArea(KnyttPoint area)
    {
        return this.Areas.Areas[area];
    }

    public static KnyttPoint getAreaCoords(Vector2 global_pos)
    {
        return new KnyttPoint((int)(global_pos.x / (GDKnyttAssetManager.TILE_WIDTH * KnyttArea.AREA_WIDTH)),
                              (int)(global_pos.y / (GDKnyttAssetManager.TILE_HEIGHT * KnyttArea.AREA_HEIGHT)));
    }

    public GDKnyttArea instantiateArea(KnyttPoint point)
    {
        var area = this.KWorld.getArea(point) ?? new KnyttArea(point, KWorld);
        var area_node = this.area_scene.Instance() as GDKnyttArea;
        area_node.loadArea(this, area);
        this.GetNode("Areas").AddChild(area_node);
        return area_node;
    }

    public void createFakeObjects()
    {
        if (!GDKnyttSettings.SeamlessScroll) { return; }
        foreach (var area in Areas.Areas.Values)
        {
            if (area == Game.CurrentArea)
            {
                area.removeFakeObjectLayer();
            }
            else
            {
                area.createFakeObjectLayer();
            }
        }
    }
}
