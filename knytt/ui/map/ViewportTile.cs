using Godot;
using System.Collections.Generic;
using YKnyttLib;

public class ViewportTile : ViewportContainer
{
    private string filename;
    private KnyttPoint coord;
    private List<GDKnyttBackground> added_backgrounds = new List<GDKnyttBackground>();
    private List<GDAreaTiles> added_tiles = new List<GDAreaTiles>();
    private List<KnyttPoint> added_areas = new List<KnyttPoint>();
    public static readonly int TILE_SCALE = 2;
    public static readonly Vector2 TILE_SIZE = TILE_SCALE * new Vector2(MapPanel.XSIZE, MapPanel.YSIZE);

    public void init(KnyttPoint coord, string filename, Texture tex = null)
    {
        this.coord = coord;
        this.filename = filename;
        refresh(tex);
    }

    public void refresh(Texture tex = null)
    {
        if (!new Directory().FileExists(filename)) { return; }
        GetNode<TextureRect>("Viewport/TextureRect").Texture = 
            tex ?? GDKnyttAssetManager.loadTexture(GDKnyttAssetManager.loadFile(filename));
    }

    public void addArea(GDKnyttArea area)
    {
        if (added_areas.Contains(area.Area.MapPosition) || area.Area.Empty) { return; }
        
        Vector2 pos = new Vector2(
            TILE_SIZE.x * (area.Area.MapPosition.x % MapPanel.SCALE), 
            240 * TILE_SCALE - TILE_SIZE.y * (area.Area.MapPosition.y % MapPanel.SCALE));
        var image = GetNode<TextureRect>("Viewport/TextureRect").Texture?.GetData();
        image?.Lock();
        if (image != null && image.GetPixel((int)pos.x, 240 * TILE_SCALE - (int)pos.y).a > 0) { return; }

        Viewport vp = GetNode<Viewport>("Viewport");
        var bg = area.GetNode("Background").Duplicate() as GDKnyttBackground;
        var tiles = area.Tiles.Duplicate() as GDAreaTiles;
        bg.Scale = tiles.Scale = new Vector2(1, -1) / MapPanel.SCALE * TILE_SCALE;
        bg.Position = tiles.Position = pos;
        vp.AddChild(bg);
        vp.AddChild(tiles);

        added_backgrounds.Add(bg);
        added_tiles.Add(tiles);
        added_areas.Add(area.Area.MapPosition);
    }

    public bool dump()
    {
        if (added_backgrounds.Count == 0) { return false; }
        GetNode<Viewport>("Viewport").GetTexture().GetData().SavePng(filename);
        return true;
    }

    public void reset()
    {
        foreach (var bg in added_backgrounds) { bg.QueueFree(); }
        foreach (var tiles in added_tiles) { tiles.QueueFree(); }
        added_backgrounds.Clear();
        added_tiles.Clear();
        added_areas.Clear();
    }

    public ViewportTexture getTexture()
    {
        return GetNode<Viewport>("Viewport").GetTexture();
    }
}
