using Godot;
using System.Collections.Generic;
using System.Text;
using YKnyttLib;
using YKnyttLib.Logging;
using YUtil.Collections;
using System.Linq;
using System;

public class GDKnyttAssetManager
{
    public const int TILESET_WIDTH = 16;
    public const int TILESET_HEIGHT = 8;
    public const int TILE_WIDTH = 24;
    public const int TILE_HEIGHT = 24;

    public GDKnyttWorld GDWorld { get; }

    ObjectCache<int, TileSet> TileSetCache;
    ObjectCache<int, Texture> GradientCache;
    ObjectCache<int, AudioStream> SongCache;
    ObjectCache<int, AudioStream> AmbianceCache;
    ObjectCache<KnyttPoint, GDKnyttObjectBundle> ObjectCache;

    public GDKnyttAssetManager(GDKnyttWorld gdworld, int tile_cache, int gradient_cache, int song_cache, int ambiance_cache, int object_cache)
    {
        this.GDWorld = gdworld;
        TileSetCache  = new ObjectCache<int, TileSet>(tile_cache)         { OnCreate = buildTileSet };
        GradientCache = new ObjectCache<int, Texture>(gradient_cache)     { OnCreate = buildGradient };
        SongCache     = new ObjectCache<int, AudioStream>(song_cache)     { OnCreate = buildSong };
        AmbianceCache = new ObjectCache<int, AudioStream>(ambiance_cache) { OnCreate = buildAmbiance };
        ObjectCache   = new ObjectCache<KnyttPoint, GDKnyttObjectBundle>(object_cache) 
                        { OnCreate = GDKnyttObjectFactory.buildKnyttObject };
    }

    public TileSet getTileSet(int num) { return TileSetCache.IncObject(num); }
    public void returnTileSet(int num) { TileSetCache.DecObject(num); }

    public Texture getGradient(int num) { return GradientCache.IncObject(num); }
    public void returnGradient(int num) { GradientCache.DecObject(num); }

    public AudioStream getSong(int num) { return SongCache.IncObject(num); }
    public void returnSong(int num) { if (SongCache.getObjectCount(num) > 0) SongCache.DecObject(num); } // TODO: disbalanced sometimes

    public AudioStream getAmbiance(int num) { return AmbianceCache.IncObject(num); }
    public void returnAmbiance(int num) { AmbianceCache.DecObject(num); }

    public GDKnyttObjectBundle GetObject(KnyttPoint object_id) { return ObjectCache.IncObject(object_id); }
    public void returnObject(KnyttPoint object_id) { ObjectCache.DecObject(object_id); }

    private TileSet buildTileSet(int num)
    {
        string cached_path = $"user://Cache/{GDWorld.KWorld.WorldDirectoryName}/Tileset{num}.res";
        if (new File().FileExists(cached_path)) { return ResourceLoader.Load<TileSet>(cached_path); }

        var texture = GDWorld.KWorld.getWorldTexture($"Tilesets/Tileset{num}.png");
        switch (texture)
        {
            case Texture t:
                // Preprocess the texture if no alpha channel
                TileSet new_tileset = makeTileset(t.HasAlpha() ? t : preprocessTilesetTexture(t));
                ensureDirExists($"user://Cache/{GDWorld.KWorld.WorldDirectoryName}");
                ResourceSaver.Save(cached_path, new_tileset, ResourceSaver.SaverFlags.Compress);
                return new_tileset;
            case TileSet ts: return ts;
            default: return null;
        }
    }

    private Texture buildGradient(int num)
    {
        return (Texture)GDWorld.KWorld.getWorldTexture($"Gradients/Gradient{num}.png");
    }

    public AudioStream buildSong(int num)
    {
        if (num == 0) { return null; }
        if (num < 0) { return (AudioStream)GDWorld.KWorld.getWorldSound($"Music/Intro{-num}.ogg", loop: false); }
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

    public static Texture loadExternalTexture(string path)
    {
        if (!new File().FileExists(path)) { return null; }
        var image = new Image();
        var error = image.Load(path);
        if (error != Error.Ok) { return null; }
        return image2Texture(image);
    }

    public static Texture loadInternalTexture(string path)
    {
        return ResourceLoader.Exists(path) ? ResourceLoader.Load<Texture>(path) : null;
    }

    public static Texture loadTexture(byte[] buffer)
    {
        if (buffer == null || buffer.Length == 0) { return null; }
        var image = new Image();
        var error = image.LoadPngFromBuffer(buffer);
        if (error != Error.Ok) { return null; }
        return image2Texture(image);
    }

    public static TileSet loadInternalTileset(string path)
    {
        return ResourceLoader.Exists(path) ? ResourceLoader.Load<TileSet>(path) : null;
    }

    public static AudioStream loadInternalSound(string path, bool loop)
    {
        var stream = ResourceLoader.Exists(path) ? ResourceLoader.Load<AudioStreamOGGVorbis>(path) : null;
        if (stream != null) { stream.Loop = loop; }
        return stream;
    }

    public static AudioStream loadExternalSound(string path, bool loop)
    {
        return loadOGG(loadFile(path), loop);
    }

    private static Texture image2Texture(Image image)
    {
        var texture = new ImageTexture();
        texture.CreateFromImage(image, (int)Texture.FlagsEnum.Repeat);
        var image_back = texture.GetData();
        if (image_back == null || texture.GetWidth() != image_back.GetWidth() || 
            texture.GetHeight() != image_back.GetHeight()) { return null; }
        return texture;
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
        if (!new File().FileExists(path)) { return null; }
        var f = new File();
        f.Open(path, File.ModeFlags.Read); // case insensitive search for Unix FSs is impossible now
        var buffer = f.GetBuffer((int)f.GetLen());
        f.Close();
        return buffer;
    }

    public static AudioStream loadOGG(byte[] buffer, bool loop = false)
    {
        return new AudioStreamOGGVorbis() { Data = buffer, Loop = loop };
    }

    public static void ensureDirExists(string dir_name)
    {
        var dir = new Directory();
        if (!dir.DirExists(dir_name)) { dir.MakeDirRecursive(dir_name); }
    }

    public static Texture preprocessTilesetTexture(Texture texture, Color? from = null)
    {
        var image = texture.GetData();
        if (image == null) { return texture; }

        if (image.DetectAlpha() == Image.AlphaMode.None) { image.Convert(Image.Format.Rgba8); }

        if (replaceColor(image, from ?? new Color(1f, 0f, 1f), new Color(0f, 0f, 0f, 0f)))
        {
            var it = new ImageTexture();
            it.CreateFromImage(image, (int)Texture.FlagsEnum.Repeat);
            texture = it;
        }

        return texture;
    }

    public static TileSet makeTileset(Texture texture)
    {
        BitMap bitmap = new BitMap();
        bitmap.CreateFromImageAlpha(texture.GetData(), .001f);
        
        var ts = new TileSet();

        int i = 0;
        for (int y = 0; y < TILESET_HEIGHT; y++)
        {
            for (int x = 0; x < TILESET_WIDTH; x++)
            {
                ts.CreateTile(i);
                ts.TileSetTexture(i, texture);
                var region = new Rect2(x * TILE_WIDTH, y * TILE_HEIGHT, TILE_WIDTH, TILE_HEIGHT);
                ts.TileSetRegion(i, region);

                var polygons = tilePolygons(bitmap, region);
                int c = 0;

                foreach (Vector2[] polygon in polygons)
                {
                    Vector2[] p = polygon;
                    bool convex = isConvex(p);
                    if (!convex)
                    {
                        if (!Geometry.IsPolygonClockwise(p)) { Array.Reverse(p); }
                        p = smoothPolygon(p);
                        if (p == null) { GD.Print($"Error in smoothing {x}, {y}"); continue; } // should never happen
                        convex = isConvex(p);
                    }

                    if (convex)
                    {
                        var collision = new ConvexPolygonShape2D();
                        collision.SetPointCloud(p);
                        ts.TileSetShape(i, c++, collision);
                    }
                    else
                    {
                        int[] triangles = Geometry.TriangulatePolygon(p);
                        for (int t = 0; t < triangles.Length; t += 3)
                        {
                            Vector2[] triangle = { p[triangles[t]], p[triangles[t + 1]], p[triangles[t + 2]] };
                            var collision = new ConvexPolygonShape2D();
                            collision.SetPointCloud(triangle);
                            ts.TileSetShape(i, c++, collision);
                        }
                    }
                }
                i++;
            }
        }

        for (int y = 0; y < TILESET_HEIGHT; y++)
        {
            for (int x = 0; x < TILESET_WIDTH; x++)
            {
                ts.CreateTile(i);
                ts.TileSetTexture(i, texture);
                var region = new Rect2(x * TILE_WIDTH, y * TILE_HEIGHT, TILE_WIDTH, TILE_HEIGHT);
                ts.TileSetRegion(i, region);
                i++;
            }
        }

        return ts;
    }

    private static IEnumerable<Vector2[]> tilePolygons(BitMap bitmap, Rect2 region)
    {
        // Makes points of contact thicker. Some polygons disappear without this (Godot bug).
        for (float i = region.Position.y; i < region.End.y - 1; i++)
        {
            bool ulbit = bitmap.GetBit(new Vector2(region.Position.x, i));
            bool dlbit = bitmap.GetBit(new Vector2(region.Position.x, i + 1));
            for (float j = region.Position.x + 1; j < region.End.x; j++)
            {
                bool urbit = bitmap.GetBit(new Vector2(j, i));
                bool drbit = bitmap.GetBit(new Vector2(j, i + 1));

                if (ulbit && !urbit && !dlbit && drbit)
                {
                    bitmap.SetBit(new Vector2(j, i), true);
                    bitmap.SetBit(new Vector2(j - 1, i + 1), true);
                }

                if (!ulbit && urbit && dlbit && !drbit)
                {
                    bitmap.SetBit(new Vector2(j - 1, i), true);
                    bitmap.SetBit(new Vector2(j, i + 1), true);
                }

                ulbit = urbit;
                dlbit = drbit;
            }
        }

        var polygons = bitmap.OpaqueToPolygons(region, 0.99f).Cast<Vector2[]>();
        // I have no idea why it's adding y*48 to y coordinates...
        return polygons.Select(p => p.Select(v => new Vector2(v.x, v.y - (region.Position.y * 2))).ToArray());
    }

    private static bool isConvex(Vector2[] vertices)
    {
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
    }

    private const float MIN_RECESS_WIDTH = 8;
    private const float MAX_RECESS_DEPTH = -2;
    private const float MIN_HILL_HEIGHT = 2;

    private static Vector2[] smoothPolygon(Vector2[] full_polygon)
    {
        var convex_hull = Geometry.ConvexHull2d(full_polygon).Reverse().ToList();

        int result_size = 0;
        while (result_size != convex_hull.Count)
        {
            result_size = convex_hull.Count;

            int fpi = Array.IndexOf(full_polygon, convex_hull[0]);
            for (int chi = 0; chi < convex_hull.Count - 1; chi++)
            {
                if (convex_hull.Count - 1 > full_polygon.Count()) { return null; } // should never happen

                Vector2 ch_from = convex_hull[chi], ch_to = convex_hull[chi + 1];
                Vector2 before_recess = ch_from, last_shallow = ch_from;
                Vector2? recess = null;
                float recess_depth = -24;

                for (fpi++; ; fpi++) // iterate between convex hull points
                {
                    Vector2 fpp = full_polygon[fpi % full_polygon.Count()];
                    if (fpp == ch_to && recess == null) { break; }

                    float height = distanceToLine(fpp, ch_from, ch_to);
                    if (height > MIN_HILL_HEIGHT) // additional convex vertex - just add it
                    {
                        convex_hull.Insert(chi + 1, fpp);
                        break;
                    }
                    
                    if (height < MAX_RECESS_DEPTH)
                    {
                        if (height > recess_depth) // select shallowest recess
                        {
                            recess_depth = height;
                            recess = fpp;
                            before_recess = last_shallow;
                        }
                    }
                    else
                    {
                        if (recess != null)
                        {
                            if (before_recess.DistanceTo(fpp) > MIN_RECESS_WIDTH)
                            {
                                convex_hull.Insert(++chi, recess.Value); // add selected recess (if it's wide enough)
                                for (; full_polygon[fpi % full_polygon.Count()] != ch_to; fpi++);
                                break;
                            }
                            if (fpp == ch_to) { break; }
                            recess = null;
                            recess_depth = -24;
                        }
                        last_shallow = fpp;
                    }
                }
            }
        }
        return convex_hull.ToArray();
    }

    private static float crossProduct(Vector2 va, Vector2 vb, Vector2 vc)
    {
        return (va.x - vb.x) * (vc.y - vb.y) - (va.y - vb.y) * (vc.x - vb.x);
    }

    public static float distanceToLine(Vector2 p, Vector2 lp1, Vector2 lp2)
    {
        return crossProduct(lp2, lp1, p) / lp1.DistanceTo(lp2);
    }

    public static bool replaceColor(Image image, Color old_color, Color new_color)
    {
        if (old_color == new_color) { return false; }
        bool replaced = false;

        image.Lock();
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
        image.Unlock();

        return replaced;
    }

    // Call this in any level-optimizing procedure (level load screen, post-download processing, special button).
    public static void compileInternalTileset(KnyttWorld world, bool recompile)
    {
        GDKnyttDataStore.ProgressHint = "Compiling tilesets...";
        ensureDirExists($"user://Cache/{world.WorldDirectoryName}");

        for (int num = 0; num < 256; num++)
        {
            GDKnyttDataStore.ProgressHint = $"Compiling tileset #{num}...";
            string tileset_path = $"Tilesets/Tileset{num}.png";
            if (!world.worldFileExists(tileset_path)) { continue; }

            string cached_path = $"user://Cache/{world.WorldDirectoryName}/Tileset{num}.res";
            if (!recompile && new File().FileExists(cached_path)) { continue; }

            var texture = world.getWorldTexture(tileset_path);
            if (texture is Texture t)
            {
                t = preprocessTilesetTexture(t);
                ResourceSaver.Save(cached_path, makeTileset(t), ResourceSaver.SaverFlags.Compress);
            }
        }
        GDKnyttDataStore.ProgressHint = "Compiling finished.";
    }

    // Call this function at start to compile default tilesets
    public static void compileTileset()
    {
        ensureDirExists($"user://tilesets");
        for (int i = 0; i < 256; i++)
        {
            KnyttLogger.Info($"Compiling tileset #{i}");
            var texture = loadInternalTexture($"res://knytt/data/Tilesets/Tileset{i}.png");
            var tileset = makeTileset(texture);
            GD.PrintErr(ResourceSaver.Save($"user://tilesets/Tileset{i}.png.res", tileset, ResourceSaver.SaverFlags.Compress));
        }
    }
}
