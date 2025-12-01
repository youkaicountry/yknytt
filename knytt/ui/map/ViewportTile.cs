using Godot;
using System.Collections.Generic;
using System.Linq;
using YKnyttLib;

public class ViewportTile : ViewportContainer
{
    private string filename;
    private KnyttPoint coord;
    private List<GDKnyttBackground> added_backgrounds = new List<GDKnyttBackground>();
    private List<GDAreaTiles> added_tiles = new List<GDAreaTiles>();
    private List<TileMap> added_objects = new List<TileMap>();
    private List<KnyttPoint> added_areas = new List<KnyttPoint>();

    private PackedScene objects_tilemap;
    private static List<KnyttPoint> VISIBLE_OBJECT_IDS = Enumerable.Range(1, 25).Select(i => new KnyttPoint(1, i))
        .Concat(new List<KnyttPoint>() { // faster than tile names
            new KnyttPoint(15, 1), new KnyttPoint(15, 2), new KnyttPoint(15, 3), new KnyttPoint(15, 4), 
            new KnyttPoint(15, 6), new KnyttPoint(15, 12), new KnyttPoint(15, 37), new KnyttPoint(15, 38), new KnyttPoint(12, 5)
        }).ToList();

    public static readonly int TILE_SCALE = 2;
    public static readonly Vector2 TILE_SIZE = TILE_SCALE * new Vector2(MapPanel.XSIZE, MapPanel.YSIZE);

    public void init(KnyttPoint coord, string filename, Texture tex = null)
    {
        this.coord = coord;
        this.filename = filename;
        objects_tilemap = ResourceLoader.Load<PackedScene>("res://knytt/ui/map/MapObjectsTileset.tscn");
        refresh(tex);
    }

    public void refresh(Texture tex = null)
    {
        if (filename.StartsWith("res://") ? 
                !ResourceLoader.Exists(filename) : 
                !new File().FileExists(filename)) { return; }

        GetNode<TextureRect>("Viewport/TextureRect").Texture = tex ?? 
            (filename.StartsWith("res://") ?
                ResourceLoader.Load<Texture>(filename) :
                GDKnyttAssetManager.loadTexture(GDKnyttAssetManager.loadFile(filename)));
    }

    public void addArea(GDKnyttArea area)
    {
        if (added_areas.Contains(area.Area.MapPosition) || area.Area.Empty) { return; }
        
        KnyttPoint tile_coord = area.Area.MapPosition % new KnyttPoint(MapPanel.SCALE, MapPanel.SCALE);
        Vector2 pos = new Vector2(TILE_SIZE.x * tile_coord.x, 240 * TILE_SCALE - TILE_SIZE.y * tile_coord.y);
        var image = GetNode<TextureRect>("Viewport/TextureRect").Texture?.GetData();
        image?.Lock();
        if (image != null && image.GetPixel((int)pos.x, 240 * TILE_SCALE - (int)pos.y).a > 0) { return; }

        Viewport vp = GetNode<Viewport>("Viewport");
        var bg = area.Background.Duplicate() as GDKnyttBackground;
        var tiles = area.Tiles.Duplicate() as GDAreaTiles;
        var objects = createObjectTiles(area);
        bg.Scale = tiles.Scale = objects.Scale = new Vector2(1, -1) / MapPanel.SCALE * TILE_SCALE;
        bg.Position = tiles.Position = objects.Position = pos;
        bg.ZIndex = -1;
        vp.AddChild(bg);
        vp.AddChild(tiles);
        vp.AddChild(objects);

        added_backgrounds.Add(bg);
        added_tiles.Add(tiles);
        added_objects.Add(objects);
        added_areas.Add(area.Area.MapPosition);
    }

    private TileMap createObjectTiles(GDKnyttArea area)
    {
        var tilemap = objects_tilemap.Instance<TileMap>();
        foreach (GDKnyttBaseObject obj in area.Objects.findObjects(VISIBLE_OBJECT_IDS))
        {
            if (obj.ObjectID.x == 12 && !obj.Juni.Powers.getPower(JuniValues.PowerNames.Eye)) { continue; }
            tilemap.SetCell(obj.Coords.x, obj.Coords.y, 0, 
                autotileCoord: new Vector2(VISIBLE_OBJECT_IDS.IndexOf(obj.ObjectID), 0));
        }
        return tilemap;
    }

    public bool dump()
    {
        if (added_backgrounds.Count == 0) { return false; }
        Error? err = GetNodeOrNull<Viewport>("Viewport")?.GetTexture()?.GetData()?.SavePng(filename);
        return err == Error.Ok;
    }

    public void reset()
    {
        foreach (var bg in added_backgrounds) { bg.QueueFree(); }
        foreach (var tiles in added_tiles) { tiles.QueueFree(); }
        foreach (var objects in added_objects) { objects.QueueFree(); }
        added_backgrounds.Clear();
        added_tiles.Clear();
        added_objects.Clear();
        added_areas.Clear();
    }

    public ViewportTexture getTexture()
    {
        return GetNode<Viewport>("Viewport").GetTexture();
    }
}
