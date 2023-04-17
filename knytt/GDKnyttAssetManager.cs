using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using YKnyttLib;
using YKnyttLib.Logging;
using YUtil.Collections;
using System.Linq;

public partial class GDKnyttAssetManager
{
    public const int TILESET_WIDTH = 16;
    public const int TILESET_HEIGHT = 8;
    public const int TILE_WIDTH = 24;
    public const int TILE_HEIGHT = 24;

    public GDKnyttWorld GDWorld { get; }
    private Dictionary<string, string> Directories { get; }

    ObjectCache<int, TileSet> TileSetCache;
    ObjectCache<int, Texture2D> GradientCache;
    ObjectCache<int, AudioStream> SongCache;
    ObjectCache<int, AudioStream> AmbianceCache;
    ObjectCache<KnyttPoint, GDKnyttObjectBundle> ObjectCache;

    public GDKnyttAssetManager(GDKnyttWorld gdworld, int tile_cache, int gradient_cache, int song_cache, int ambiance_cache, int object_cache)
    {
        this.GDWorld = gdworld;
        Directories = new Dictionary<string, string>();

        TileSetCache = new ObjectCache<int, TileSet>(tile_cache);
        TileSetCache.OnCreate = (int num) => buildTileSet(num);

        GradientCache = new ObjectCache<int, Texture2D>(gradient_cache);
        GradientCache.OnCreate = (int num) => buildGradient(num);

        SongCache = new ObjectCache<int, AudioStream>(song_cache);
        SongCache.OnCreate = (int num) => buildSong(num);

        AmbianceCache = new ObjectCache<int, AudioStream>(ambiance_cache);
        AmbianceCache.OnCreate = (int num) => buildAmbiance(num);

        ObjectCache = new ObjectCache<KnyttPoint, GDKnyttObjectBundle>(object_cache);
        ObjectCache.OnCreate = (KnyttPoint id) => GDKnyttObjectFactory.buildKnyttObject(id);
    }

    public TileSet getTileSet(int num) { return TileSetCache.IncObject(num); }
    public void returnTileSet(int num) { TileSetCache.DecObject(num); }

    public Texture2D getGradient(int num) { return GradientCache.IncObject(num); }
    public void returnGradient(int num) { GradientCache.DecObject(num); }

    public AudioStream getSong(int num) { return SongCache.IncObject(num); }
    public void returnSong(int num) { SongCache.DecObject(num); }

    public AudioStream getAmbiance(int num) { return AmbianceCache.IncObject(num); }
    public void returnAmbiance(int num) { AmbianceCache.DecObject(num); }

    public GDKnyttObjectBundle GetObject(KnyttPoint object_id) { return ObjectCache.IncObject(object_id); }
    public void returnObject(KnyttPoint object_id) { ObjectCache.DecObject(object_id); }

    private TileSet buildTileSet(int num)
    {
        string cached_path = $"user://Cache/{GDWorld.KWorld.WorldDirectoryName}/Tileset{num}.res";
        if (FileAccess.FileExists(cached_path)) { return ResourceLoader.Load<TileSet>(cached_path); }

        var texture = GDWorld.KWorld.getWorldTexture($"Tilesets/Tileset{num}.png");
        switch (texture)
        {
            case Texture2D t:
                // Preprocess the texture if no alpha channel
                TileSet new_tileset = makeTileset(t.HasAlpha() ? t : preprocessTilesetTexture(t), true);
                ensureDirExists($"user://Cache/{GDWorld.KWorld.WorldDirectoryName}");
                //ResourceSaver.Save(new_tileset, cached_path, ResourceSaver.SaverFlags.Compress);
                return new_tileset;
            case TileSet ts: return ts;
            default: return null;
        }
    }

    private Texture2D buildGradient(int num)
    {
        return (Texture2D)GDWorld.KWorld.getWorldTexture($"Gradients/Gradient{num}.png");
    }

    public AudioStream buildSong(int num)
    {
        if (num == 0) { return null; }
        return (AudioStream)GDWorld.KWorld.getWorldSound($"Music/Song{num}.ogg", loop: isSongLooped(num));
    }

    private bool isSongLooped(int num)
    {
        return GDWorld.KWorld.INIData["Loop Music"][num.ToString()]?.ToLower() == "true";
    }

    public AudioStream buildAmbiance(int num)
    {
        if (num == 0) { return null; }
        return (AudioStream)GDWorld.KWorld.getWorldSound($"Ambiance/Ambi{num}.ogg", loop: true);
    }

    public static Texture2D loadExternalTexture(string path)
    {
        if (!FileAccess.FileExists(path)) { return null; }
        var image = new Image();
        var error = image.Load(path);
        if (error != Error.Ok) { return null; }
        return ImageTexture.CreateFromImage(image);
    }

    public static Texture2D loadInternalTexture(string path)
    {
        return ResourceLoader.Exists(path) ? ResourceLoader.Load<Texture2D>(path) : null;
    }

    public static Texture2D loadTexture(byte[] buffer)
    {
        if (buffer == null || buffer.Length == 0) { return null; }
        var image = new Image();
        var error = image.LoadPngFromBuffer(buffer);
        if (error != Error.Ok) { return null; }
        return ImageTexture.CreateFromImage(image);
    }

    public static TileSet loadInternalTileset(string path)
    {
        return ResourceLoader.Exists(path) ? ResourceLoader.Load<TileSet>(path) : null;
    }

    public static AudioStream loadInternalSound(string path, bool loop)
    {
        var stream = ResourceLoader.Exists(path) ? ResourceLoader.Load<AudioStreamOggVorbis>(path) : null;
        if (stream != null) { stream.Loop = loop; }
        return stream;
    }

    public static AudioStream loadExternalSound(string path, bool loop)
    {
        return loadOGG(loadFile(path), loop);
    }

    public static string loadTextFile(byte[] buffer)
    {
        ASCIIEncoding encoder = new ASCIIEncoding();
        return encoder.GetString(buffer, 0, buffer.Length);
    }

    public static string loadTextFile(string path)
    {
        return loadTextFile(loadFile(path));
    }

    public static byte[] loadFile(string path)
    {
        if (!FileAccess.FileExists(path)) { return null; }
        using var f = FileAccess.Open(path, FileAccess.ModeFlags.Read); // case insensitive search for Unix FSs is impossible now
        var buffer = f.GetBuffer((int)f.GetLength());
        return buffer;
    }

    public static AudioStream loadOGG(byte[] buffer, bool loop = false)
    {
        GD.Print("Loading OGG in Godot 4 is not supported yet");
        return null;
        /*var stream = new AudioStreamOggVorbis();
        stream.Data = buffer;
        stream.Loop = loop;
        return stream;*/
    }

    public static void ensureDirExists(string dir_name)
    {
        if (!DirAccess.DirExistsAbsolute(dir_name)) { DirAccess.MakeDirRecursiveAbsolute(dir_name); }
    }

    public static string extractFilename(string full_path)
    {
        return full_path.Substring(full_path.LastIndexOfAny("/\\".ToCharArray()) + 1);
    }

    public static Texture2D preprocessTilesetTexture(Texture2D texture, Color? from = null)
    {
        var image = texture.GetImage();
        if (image == null) { return texture; }

        if (image.DetectAlpha() == Image.AlphaMode.None) { image.Convert(Image.Format.Rgba8); }

        if (replaceColor(image, from ?? new Color(1f, 0f, 1f), new Color(0f, 0f, 0f, 0f)))
        {
            texture = ImageTexture.CreateFromImage(image);
        }

        return texture;
    }

    public static TileSet makeTileset(Texture2D texture, bool collisions)
    {
        Bitmap original_bitmap = null;
        Bitmap bitmap = null;

        var ts = new TileSet();
        ts.TileSize = new Vector2I(TILE_WIDTH, TILE_HEIGHT);
        ts.AddPhysicsLayer();
        ts.SetPhysicsLayerCollisionLayer(0, 2 | 2048);
        var tsas_clear = new TileSetAtlasSource();
        var tsas_physics = new TileSetAtlasSource();
        tsas_clear.Texture           = tsas_physics.Texture           = texture;
        tsas_clear.TextureRegionSize = tsas_physics.TextureRegionSize = new Vector2I(TILE_WIDTH, TILE_HEIGHT);
        tsas_clear.UseTexturePadding = tsas_physics.UseTexturePadding = false;
        ts.AddSource(tsas_clear);
        ts.AddSource(tsas_physics);

        if (collisions)
        {
            var image = texture.GetImage();
            original_bitmap = new Bitmap();
            original_bitmap.CreateFromImageAlpha(image, .001f);
            
            // bitmap with borders is needed to shrink mask later
            bitmap = new Bitmap();
            bitmap.Create(new Vector2I((TILE_WIDTH + 2) * TILESET_WIDTH, (TILE_HEIGHT + 2) * TILESET_HEIGHT));
            bitmap.SetBitRect(new Rect2I(Vector2I.Zero, bitmap.GetSize()), true);
        }

        for (int y = 0; y < TILESET_HEIGHT; y++)
        {
            for (int x = 0; x < TILESET_WIDTH; x++)
            {
                tsas_clear.CreateTile(new Vector2I(x, y));
                tsas_physics.CreateTile(new Vector2I(x, y));

                if (collisions)
                {
                    var tile_data = tsas_physics.GetTileData(new Vector2I(x, y), 0);
                    
                    for (int m = 0; m < TILE_WIDTH; m++)
                    {
                        for (int n = 0; n < TILE_HEIGHT; n++)
                        {
                            bitmap.SetBit(x * (TILE_WIDTH + 2) + m + 1, y * (TILE_HEIGHT + 2) + n + 1, 
                                original_bitmap.GetBit(x * TILE_WIDTH + m, y * TILE_HEIGHT + n));
                        }
                    }

                    var bitmap_region = new Rect2I(x * (TILE_WIDTH + 2) + 1, y * (TILE_HEIGHT + 2) + 1, TILE_WIDTH, TILE_HEIGHT);
                    var polygons = tilePolygons(bitmap, bitmap_region);

                    foreach (Vector2[] polygon in polygons)
                    {
                        (bool convex, bool clockwise) = isConvex(polygon);
                        tile_data.AddCollisionPolygon(0);
                        tile_data.SetCollisionPolygonPoints(0, tile_data.GetCollisionPolygonsCount(0) - 1, convex ? polygon : smoothPolygon(polygon, clockwise));
                    }
                }
            }
        }
        return ts;
    }

    private static IEnumerable<Vector2[]> tilePolygons(Bitmap bitmap, Rect2I region)
    {
        /*if (debug) // Print bitmap mask: before
        for (float n = region.Position.Y; n < region.End.Y; n++)
        GD.Print(String.Join("", Enumerable.Range((int)region.Position.X, TILE_WIDTH).Select(m => bitmap.GetBit(new Vector2(m, n)) ? "1" : "0")));
        if (debug) GD.Print("");*/

        // Smooth mask a little: grow true bits, then shrink. This will fill inner void pixels and reduce number of polygons.
        // Also workaround for https://github.com/godotengine/godot/issues/31675
        bitmap.GrowMask(1, region);
        bitmap.GrowMask(-1, new Rect2I(region.Position.X - 1, region.Position.Y - 1, TILE_WIDTH + 2, TILE_HEIGHT + 2));

        /*if (debug) // Print bitmap mask: after
        for (float n = region.Position.Y; n < region.End.Y; n++)
        GD.Print(String.Join("", Enumerable.Range((int)region.Position.X, TILE_WIDTH).Select(m => bitmap.GetBit(new Vector2(m, n)) ? "1" : "0")));*/

        var polygons = bitmap.OpaqueToPolygons(region, 0.99f);// as IEnumerable).Cast<Vector2[]>();
        // I have no idea why it's adding y*48 to y coordinates...
        return polygons.Select(p => p.Select(v => new Vector2(v.X - 12, v.Y - 12)).ToArray());
        //GD.Print("polygons");
        //foreach (var p in polygons) { GD.Print("p"); foreach (var v in p) GD.Print(v); }
        //return polygons;
    }

    private static (bool, bool) isConvex(Vector2[] vertices)
    {
        int got_negative = 0;
        int got_positive = 0;
		float sum = 0;
        for (int a = 0; a < vertices.Length; a++)
        {
            int b = (a + 1) % vertices.Length;
            int c = (a + 2) % vertices.Length;
            float cross_product = crossProduct(vertices[a], vertices[b], vertices[c]);
            if (cross_product < 0) { got_negative += 1; }
            if (cross_product > 0) { got_positive += 1; }
			sum += (vertices[b].X - vertices[a].X) * (vertices[b].Y + vertices[a].Y);
            //if (got_negative && got_positive) { return false; }
        }
		//GD.Print($"{sum} {got_negative} {got_positive}");
		return (got_negative == 0 || got_positive == 0, sum > 0);
    }

    private static Vector2[] smoothPolygon(Vector2[] vertices, bool clockwise)
    {
        var result = new List<Vector2>();
        for (int b = 0; b < vertices.Length; b++)
        {
            Vector2 valid_a = result.Count > 0 ? result.Last() : vertices.Last();
            int c = (b + 1) % vertices.Length;
            float cross_product = crossProduct(valid_a, vertices[b], vertices[c]);
            if ((cross_product < 0 && clockwise) || (cross_product > 0 && !clockwise))
            {
                if (valid_a.DistanceTo(vertices[c]) < 10) { continue; }
            }
            result.Add(vertices[b]);
        }
        return result.ToArray();
    }

    /*private static bool isConvex(Vector2[] vertices)
    {
        // TODO: return true for polygons which are almost convex, or their recesses are too small
        //  (because unnecessary triangles inflate tilesets)
        bool got_negative = false;
        bool got_positive = false;
        for (int a = 0; a < vertices.Length; a++)
        {
            int b = (a + 1) % vertices.Length;
            int c = (a + 2) % vertices.Length;
            float cross_product = crossProduct(vertices[a], vertices[b], vertices[c]);
            if (cross_product < 0) { got_negative = true; }
            if (cross_product > 0) { got_positive = true; }
            if (got_negative && got_positive) { return false; }
        }
        return true;
    }*/

    private static float crossProduct(Vector2 va, Vector2 vb, Vector2 vc)
    {
        return (va.X - vb.X) * (vc.Y - vb.Y) - (va.Y - vb.Y) * (vc.X - vb.X);
    }

    public static bool replaceColor(Image image, Color old_color, Color new_color)
    {
        if (old_color == new_color) { return false; }
        bool replaced = false;

        //image.Lock();
        for (int y = 0; y < image.GetHeight(); y++)
        {
            for (int x = 0; x < image.GetWidth(); x++)
            {
                if (image.GetPixel(x, y) == old_color)
                {
                    image.SetPixel(x, y, new_color);
                    replaced = true;
                }
            }
        }
        //image.Unlock();

        return replaced;
    }

    // Call this in any level-optimizing procedure (level load screen, post-download processing, special button).
    public static void compileInternalTileset(KnyttWorld world, bool recompile)
    {
        ensureDirExists($"user://Cache/{world.WorldDirectoryName}");

        for (int num = 0; num < 256; num++)
        {
            string tileset_path = $"Tilesets/Tileset{num}.png";
            if (!world.worldFileExists(tileset_path)) { continue; }

            string cached_path = $"user://Cache/{world.WorldDirectoryName}/Tileset{num}.res";
            if (!recompile && FileAccess.FileExists(cached_path)) { continue; }

            var texture = world.getWorldTexture(tileset_path);
            if (texture is Texture2D t)
            {
                t = preprocessTilesetTexture(t);
                ResourceSaver.Save(makeTileset(t, true), cached_path, ResourceSaver.SaverFlags.Compress);
            }
        }
    }

    // Create "user://tilesets" directory and call this function at start to compile default tilesets
    public static void compileTileset()
    {
        for (int i = 0; i < 256; i++)
        {
            KnyttLogger.Info($"Compiling tileset #{i}");
            var texture = loadInternalTexture($"res://knytt/data/Tilesets/Tileset{i}.png");
            var tileset = makeTileset(texture, true);
            GD.PrintErr(ResourceSaver.Save(tileset, $"user://tilesets/Tileset{i}.png.res", ResourceSaver.SaverFlags.Compress));
        }
    }
}
