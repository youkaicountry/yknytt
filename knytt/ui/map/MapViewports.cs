using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YKnyttLib;

public class MapViewports : Node2D
{
    public Dictionary<KnyttPoint, ViewportTile> viewports = new Dictionary<KnyttPoint, ViewportTile>();
    public Dictionary<KnyttPoint, Texture> map_images = new Dictionary<KnyttPoint, Texture>();
    public Queue<KnyttPoint> latest_images = new Queue<KnyttPoint>();
    private static readonly int IMAGES_LIMIT = 50;
    private PackedScene viewport_scene;
    private KnyttWorld KWorld;
    private string cache_dir;
    private bool internal_cache;

    public override void _Ready()
    {
        viewport_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/map/ViewportTile.tscn");
    }

    public void init(KnyttWorld world)
    {
        this.KWorld = world;
        string internal_cache_dir = $"res://knytt/data/Compiled/Maps/{KWorld.WorldDirectoryName}";
        internal_cache = new Directory().DirExists(internal_cache_dir);
        cache_dir = internal_cache ? internal_cache_dir :
            GDKnyttDataStore.BaseDataDirectory.PlusFile($"Cache/{KWorld.WorldDirectoryName}");
        GDKnyttAssetManager.ensureDirExists(cache_dir);
    }

    public void destroy()
    {
        KWorld = null;
        foreach (ViewportTile c in GetChildren()) { c.QueueFree(); }
        viewports.Clear();
        map_images.Clear();
        latest_images.Clear();
    }

    private KnyttPoint getKey(KnyttPoint area_coord) => 
        area_coord / new KnyttPoint(MapPanel.SCALE, MapPanel.SCALE) * new KnyttPoint(MapPanel.SCALE, MapPanel.SCALE);

    private string getFilename(KnyttPoint key) => $"{cache_dir}/map_x{key.x}y{key.y}.png";

    public void addArea(GDKnyttArea area)
    {
        if (KWorld == null || area == null) { return; }
        var key = getKey(area.Area.MapPosition);
        if (!viewports.ContainsKey(key))
        {
            ViewportTile tile = viewport_scene.Instance<ViewportTile>();
            tile.init(key, getFilename(key), map_images.ContainsKey(key) ? map_images[key] : null);
            AddChild(tile);
            viewports.Add(key, tile);
        }
        if (!internal_cache)
        {
            viewports[key].addArea(area);
        }
    }

    public (Rect2, Texture) getArea(KnyttPoint coord)
    {
        var key = getKey(coord);
        KnyttPoint tile_coord = coord % new KnyttPoint(MapPanel.SCALE, MapPanel.SCALE);
        Rect2 src = new Rect2(new Vector2(tile_coord.x, tile_coord.y) * ViewportTile.TILE_SIZE, ViewportTile.TILE_SIZE);

        if (viewports.ContainsKey(key)) { return (src, viewports[key].getTexture()); }
        if (map_images.ContainsKey(key)) { return (src, map_images[key]); }

        var filename = getFilename(key);
        if (!new File().FileExists(filename)) { return (src, null); }
        map_images[key] = GDKnyttAssetManager.loadTexture(GDKnyttAssetManager.loadFile(filename));
        latest_images.Enqueue(key);
        if (latest_images.Count > IMAGES_LIMIT) { map_images.Remove(latest_images.Dequeue()); }
        return (src, map_images[key]);
    }

    public void loadAll()
    {
        if (KWorld == null) { return; }
        var wd = new Directory();
        var error = wd.Open(cache_dir);
        wd.ListDirBegin(skipNavigational: true);
        while (true)
        {
            string name = wd.GetNext();
            if (name.Length == 0) { break; }
            if (wd.CurrentIsDir() || !name.StartsWith("map_x") || !name.EndsWith(".png")) { continue; }
            int yp = name.IndexOf('y');
            var key = new KnyttPoint(int.Parse(name.Substring(5, yp - 5)), int.Parse(name.Substring(yp + 1, name.Length - yp - 1 - 4)));
            map_images[key] = GDKnyttAssetManager.loadTexture(GDKnyttAssetManager.loadFile(cache_dir.PlusFile(name)));
            latest_images.Enqueue(key);
        }
        wd.ListDirEnd();
    }

    public async void saveAll()
    {
        if (KWorld == null || internal_cache) { return; }
        if (IsInsideTree())
        {
            await ToSignal(GetTree(), "idle_frame"); // in case area was added right before dump
            await ToSignal(GetTree(), "idle_frame");
        }
        if (OS.GetName() == "HTML5" || OS.GetName() == "Unix") { CallDeferred("saveAllInternal"); } else { await Task.Run(saveAllInternal); }
    }

    private void saveAllInternal()
    {
        foreach (var key in viewports.Keys.ToArray())
        {
            if (viewports[key].dump())
            {
                viewports[key].reset();
                viewports[key].refresh();
            }
        }
    }
}
