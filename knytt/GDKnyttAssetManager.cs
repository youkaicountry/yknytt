using Godot;
using System.Collections.Generic;
using System.Text;
using YKnyttLib;
using YKnyttLib.Logging;
using YUtil.Collections;
using System.Linq;
using System;

public partial class GDKnyttAssetManager
{
    public const int TILESET_WIDTH = 16;
    public const int TILESET_HEIGHT = 8;
    public const int TILE_WIDTH = 24;
    public const int TILE_HEIGHT = 24;

    public GDKnyttWorld GDWorld { get; }

    ObjectCache<int, TileSet> TileSetCache;
    ObjectCache<int, Texture2D> GradientCache;
    ObjectCache<int, AudioStream> SongCache;
    ObjectCache<int, AudioStream> AmbianceCache;
    ObjectCache<KnyttPoint, GDKnyttObjectBundle> ObjectCache;

    public GDKnyttAssetManager(GDKnyttWorld gdworld, int tile_cache, int gradient_cache, int song_cache, int ambiance_cache, int object_cache)
    {
        this.GDWorld = gdworld;
        TileSetCache  = new ObjectCache<int, TileSet>(tile_cache)         { OnCreate = buildTileSet };
        GradientCache = new ObjectCache<int, Texture2D>(gradient_cache)     { OnCreate = buildGradient };
        SongCache     = new ObjectCache<int, AudioStream>(song_cache)     { OnCreate = buildSong };
        AmbianceCache = new ObjectCache<int, AudioStream>(ambiance_cache) { OnCreate = buildAmbiance };
        ObjectCache   = new ObjectCache<KnyttPoint, GDKnyttObjectBundle>(object_cache) 
                        { OnCreate = GDKnyttObjectFactory.buildKnyttObject };
    }

    public TileSet getTileSet(int num) { return TileSetCache.IncObject(num); }
    public void returnTileSet(int num) { TileSetCache.DecObject(num); }

    public Texture2D getGradient(int num) { return GradientCache.IncObject(num); }
    public void returnGradient(int num) { GradientCache.DecObject(num); }

    public AudioStream getSong(int num) { return SongCache.IncObject(num); }
    public void returnSong(int num) { if (SongCache.getObjectCount(num) > 0) SongCache.DecObject(num); } // TODO: disbalanced sometimes

    public AudioStream getAmbiance(int num) { return AmbianceCache.IncObject(num); }
    public void returnAmbiance(int num) { AmbianceCache.DecObject(num); }

    public GDKnyttObjectBundle GetObject(KnyttPoint object_id) { return ObjectCache.IncObject(object_id); }
    public void returnObject(KnyttPoint object_id) { ObjectCache.DecObject(object_id); }

    private TileSet buildTileSet(int num)
    {
        string cached_path = GDKnyttDataStore.BaseDataDirectory.PathJoin($"Cache/{GDWorld.KWorld.WorldDirectoryName}/Tileset{num}.res");
        if (FileAccess.FileExists(cached_path)) { return ResourceLoader.Load<TileSet>(cached_path); }

        var texture = GDWorld.KWorld.getWorldTexture2D($"Tilesets/Tileset{num}.png");
        switch (texture)
        {
            case Texture2D t:
                // Preprocess the texture if no alpha channel
                TileSet new_tileset = makeTileset(t.HasAlpha() ? t : preprocessTilesetTexture2D(t));
                ensureDirExists(GDKnyttDataStore.BaseDataDirectory.PathJoin($"Cache/{GDWorld.KWorld.WorldDirectoryName}"));
                // Godot 4: ResourceSaver.Save parameter order changed to (resource, path, flags)
                ResourceSaver.Save(new_tileset, cached_path, ResourceSaver.SaverFlags.Compress);
                return new_tileset;
            case TileSet ts: return ts;
            default: return null;
        }
    }

    private Texture2D buildGradient(int num)
    {
        return (Texture2D)GDWorld.KWorld.getWorldTexture2D($"Gradients/Gradient{num}.png");
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

    public static Texture2D loadExternalTexture2D(string path)
    {
        if (!FileAccess.FileExists(path)) { return null; }
        var image = new Image();
        var error = image.Load(path);
        if (error != Error.Ok) { return null; }
        return image2Texture2D(image);
    }

    public static Texture2D loadInternalTexture2D(string path)
    {
        return ResourceLoader.Exists(path) ? ResourceLoader.Load<Texture2D>(path) : null;
    }

    public static Texture2D loadTexture2D(byte[] buffer)
    {
        if (buffer == null || buffer.Length == 0) { return null; }
        var image = new Image();
        var error = image.LoadPngFromBuffer(buffer);
        if (error != Error.Ok) { return null; }
        return image2Texture2D(image);
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

    private static Texture2D image2Texture2D(Image image)
    {
        if (OS.GetName() == "Unix" && (image.GetWidth() > 8192 || image.GetHeight() > 8192)) { return null; }
        // Godot 4: CreateFromImage is now a static factory method, no flags parameter
        // Texture flags (repeat, filter) are now handled via shader/material settings
        var texture = ImageTexture.CreateFromImage(image);
        var image_back = texture.GetImage();
        if (image_back == null || texture.GetWidth() != image_back.GetWidth() ||
            texture.GetHeight() != image_back.GetHeight()) { return null; }
        return texture;
    }

    public static string loadTextFileRaw(byte[] buffer)
    {
        char[] chars = new char[buffer.Length];
        for (int i = 0; i < buffer.Length; i++) { chars[i] = (char)buffer[i]; }
        return new string(chars);
    }

    public static string loadTextFile(byte[] buffer)
    {
        var encoder = Encoding.GetEncoding(1252);
        return encoder.GetString(buffer, 0, buffer.Length);
    }

    public static string loadTextFile(string path)
    {
        return loadTextFile(loadFile(path));
    }

    public static byte[] loadFile(string path)
    {
        // Handle res:// paths for bundled resources
        if (path.StartsWith("res://"))
        {
            using var f = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            if (f == null) { return null; }
            return f.GetBuffer((long)f.GetLength());
        }
        else
        {
            // Handle regular filesystem paths
            if (!FileAccess.FileExists(path)) { return null; }
            using var f = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            if (f == null) { return null; }
            return f.GetBuffer((long)f.GetLength());
        }
    }

    public static AudioStream loadOGG(byte[] buffer, bool loop = false)
    {
        // Godot 4: AudioStreamOggVorbis loading from bytes requires creating via LoadFromBuffer
        var stream = AudioStreamOggVorbis.LoadFromBuffer(buffer);
        if (stream != null) { stream.Loop = loop; }
        return stream;
    }

    public static void ensureDirExists(string dir_name)
    {
        if (!DirAccess.DirExistsAbsolute(dir_name)) { DirAccess.MakeDirRecursiveAbsolute(dir_name); }
    }

    public static Texture2D preprocessTilesetTexture2D(Texture2D texture, Color? from = null)
    {
        var image = texture.GetImage();
        if (image == null) { return texture; }

        if (image.DetectAlpha() == Image.AlphaMode.None) { image.Convert(Image.Format.Rgba8); }

        if (replaceColor(image, from ?? new Color(1f, 0f, 1f), new Color(0f, 0f, 0f, 0f)))
        {
            // Godot 4: CreateFromImage is a static factory method
            texture = ImageTexture.CreateFromImage(image);
        }

        return texture;
    }

    public static TileSet makeTileset(Texture2D texture)
    {
        Bitmap bitmap = new Bitmap();
        bitmap.CreateFromImageAlpha(texture.GetImage(), .001f);

        var ts = new TileSet();
        ts.TileSize = new Vector2I(TILE_WIDTH, TILE_HEIGHT);

        // Add a physics layer for collision
        // Collision layer 2050 = bits 1+11 = layers 2 (JuniCollides) and 12
        ts.AddPhysicsLayer();
        ts.SetPhysicsLayerCollisionLayer(0, 2050);
        ts.SetPhysicsLayerCollisionMask(0, 0);

        // Godot 4: Create TileSetAtlasSource for atlas-based tiles
        // Source 0: tiles with collision (tile IDs 0-127)
        var sourceWithCollision = new TileSetAtlasSource();
        sourceWithCollision.Texture = texture;
        sourceWithCollision.TextureRegionSize = new Vector2I(TILE_WIDTH, TILE_HEIGHT);
        int sourceIdWithCollision = ts.AddSource(sourceWithCollision);

        // Source 1: tiles without collision (tile IDs 128-255)
        var sourceNoCollision = new TileSetAtlasSource();
        sourceNoCollision.Texture = texture;
        sourceNoCollision.TextureRegionSize = new Vector2I(TILE_WIDTH, TILE_HEIGHT);
        int sourceIdNoCollision = ts.AddSource(sourceNoCollision);

        // Create tiles with collision (source 0)
        for (int y = 0; y < TILESET_HEIGHT; y++)
        {
            for (int x = 0; x < TILESET_WIDTH; x++)
            {
                var atlasCoords = new Vector2I(x, y);
                sourceWithCollision.CreateTile(atlasCoords);

                var region = new Rect2(x * TILE_WIDTH, y * TILE_HEIGHT, TILE_WIDTH, TILE_HEIGHT);
                var polygons = tilePolygons(bitmap, region);

                // Get tile data for adding collision polygons
                TileData tileData = sourceWithCollision.GetTileData(atlasCoords, 0);
                int polygonIndex = 0;

                foreach (Vector2[] polygon in polygons)
                {
                    Vector2[] p = polygon;
                    bool convex = isConvex(p);
                    if (!convex)
                    {
                        if (!Geometry2D.IsPolygonClockwise(p)) { Array.Reverse(p); }
                        p = smoothPolygon(p);
                        if (p == null) { GD.Print($"Error in smoothing {x}, {y}"); continue; }
                        convex = isConvex(p);
                    }

                    if (convex)
                    {
                        tileData.AddCollisionPolygon(0);
                        tileData.SetCollisionPolygonPoints(0, polygonIndex++, p);
                    }
                    else
                    {
                        int[] triangles = Geometry2D.TriangulatePolygon(p);
                        for (int t = 0; t < triangles.Length; t += 3)
                        {
                            Vector2[] triangle = { p[triangles[t]], p[triangles[t + 1]], p[triangles[t + 2]] };
                            tileData.AddCollisionPolygon(0);
                            tileData.SetCollisionPolygonPoints(0, polygonIndex++, triangle);
                        }
                    }
                }
            }
        }

        // Create tiles without collision (source 1)
        for (int y = 0; y < TILESET_HEIGHT; y++)
        {
            for (int x = 0; x < TILESET_WIDTH; x++)
            {
                var atlasCoords = new Vector2I(x, y);
                sourceNoCollision.CreateTile(atlasCoords);
                // No collision polygons added - this source has no physics
            }
        }

        return ts;
    }

    private static IEnumerable<Vector2[]> tilePolygons(Bitmap bitmap, Rect2 region)
    {
        // Makes points of contact thicker. Some polygons disappear without this (Godot bug).
        // Godot 4: Bitmap.GetBit/SetBit now take (int x, int y) instead of Vector2
        for (int i = (int)region.Position.Y; i < (int)region.End.Y - 1; i++)
        {
            bool ulbit = bitmap.GetBit((int)region.Position.X, i);
            bool dlbit = bitmap.GetBit((int)region.Position.X, i + 1);
            for (int j = (int)region.Position.X + 1; j < (int)region.End.X; j++)
            {
                bool urbit = bitmap.GetBit(j, i);
                bool drbit = bitmap.GetBit(j, i + 1);

                if (ulbit && !urbit && !dlbit && drbit)
                {
                    bitmap.SetBit(j, i, true);
                    bitmap.SetBit(j - 1, i + 1, true);
                }

                if (!ulbit && urbit && dlbit && !drbit)
                {
                    bitmap.SetBit(j - 1, i, true);
                    bitmap.SetBit(j, i + 1, true);
                }

                ulbit = urbit;
                dlbit = drbit;
            }
        }

        // Godot 4: OpaqueToPolygons takes Rect2I instead of Rect2
        var polygons = bitmap.OpaqueToPolygons(new Rect2I((int)region.Position.X, (int)region.Position.Y, (int)region.Size.X, (int)region.Size.Y), 0.99f).Cast<Vector2[]>();
        // I have no idea why it's adding y*48 to y coordinates...
        return polygons.Select(p => p.Select(v => new Vector2(v.X, v.Y - (region.Position.Y * 2))).ToArray());
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
        // Godot 4: ConvexHull2d renamed to ConvexHull
        var convex_hull = Geometry2D.ConvexHull(full_polygon).Reverse().ToList();

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
        return (va.X - vb.X) * (vc.Y - vb.Y) - (va.Y - vb.Y) * (vc.X - vb.X);
    }

    public static float distanceToLine(Vector2 p, Vector2 lp1, Vector2 lp2)
    {
        return crossProduct(lp2, lp1, p) / lp1.DistanceTo(lp2);
    }

    public static bool replaceColor(Image image, Color old_color, Color new_color)
    {
        if (old_color == new_color) { return false; }
        bool replaced = false;

        // Godot 4: Lock/Unlock removed - direct pixel access is always available
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

        return replaced;
    }

    // Call this in any level-optimizing procedure (level load screen, post-download processing, special button).
    public static void compileInternalTileset(KnyttWorld world, bool recompile)
    {
        GDKnyttDataStore.ProgressHint = "Compiling tilesets...";
        ensureDirExists(GDKnyttDataStore.BaseDataDirectory.PathJoin($"Cache/{world.WorldDirectoryName}"));

        for (int num = 0; num < 256; num++)
        {
            GDKnyttDataStore.ProgressHint = $"Compiling tileset #{num}...";
            string tileset_path = $"Tilesets/Tileset{num}.png";
            if (!world.worldFileExists(tileset_path)) { continue; }

            string cached_path = GDKnyttDataStore.BaseDataDirectory.PathJoin($"Cache/{world.WorldDirectoryName}/Tileset{num}.res");
            if (!recompile && FileAccess.FileExists(cached_path)) { continue; }

            var texture = world.getWorldTexture2D(tileset_path);
            if (texture is Texture2D t)
            {
                if (!t.HasAlpha()) { t = preprocessTilesetTexture2D(t); }
                // Godot 4: ResourceSaver.Save signature is (resource, path, flags)
                ResourceSaver.Save(makeTileset(t), cached_path, ResourceSaver.SaverFlags.Compress);
            }
        }
        GDKnyttDataStore.ProgressHint = "Compiling finished.";
    }

    // Call this function at start to compile default tilesets
    public static void compileTileset()
    {
        ensureDirExists(GDKnyttDataStore.BaseDataDirectory.PathJoin("tilesets"));
        for (int i = 0; i < 256; i++)
        {
            KnyttLogger.Info($"Compiling tileset #{i}");
            var texture = loadInternalTexture2D($"res://knytt/data/Tilesets/Tileset{i}.png");
            var tileset = makeTileset(texture);
            // Godot 4: ResourceSaver.Save signature is (resource, path, flags)
            GD.PrintErr(ResourceSaver.Save(tileset, GDKnyttDataStore.BaseDataDirectory.PathJoin($"tilesets/Tileset{i}.png.res"), ResourceSaver.SaverFlags.Compress));
        }
    }
}
