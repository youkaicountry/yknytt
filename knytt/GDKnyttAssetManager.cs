using Godot;
using System.Collections.Generic;
using System.Text;
using YKnyttLib;
using YUtil.Collections;

public class GDKnyttAssetManager
{
    public const int TILESET_WIDTH = 16;
    public const int TILESET_HEIGHT = 8;
    public const int TILE_WIDTH = 24;
    public const int TILE_HEIGHT = 24;

    public GDKnyttWorld GDWorld { get; }
    private Dictionary<string, string> Directories { get; }

    ObjectCache<int, TileSet> TileSetCache;
    ObjectCache<int, Texture> GradientCache;
    ObjectCache<int, AudioStream> SongCache;
    ObjectCache<int, AudioStream> AmbianceCache;
    ObjectCache<KnyttPoint, GDKnyttObjectBundle> ObjectCache;

    public GDKnyttAssetManager(GDKnyttWorld gdworld, int tile_cache, int gradient_cache, int song_cache, int ambiance_cache, int object_cache)
    {
        this.GDWorld = gdworld;
        Directories = new Dictionary<string, string>();

        TileSetCache = new ObjectCache<int, TileSet>(tile_cache);
        TileSetCache.OnCreate = (int num) => buildTileSet(num);

        GradientCache = new ObjectCache<int, Texture>(gradient_cache);
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

    public Texture getGradient(int num) { return GradientCache.IncObject(num); }
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
        if (new File().FileExists(cached_path)) { return ResourceLoader.Load<TileSet>(cached_path); }

        var texture = GDWorld.KWorld.getWorldTexture($"Tilesets/Tileset{num}.png");
        switch (texture)
        {
            case Texture t:
                // Preprocess the texture if no alpha channel
                TileSet new_tileset = makeTileset(t.HasAlpha() ? t : preprocessTilesetTexture(t), true);
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

    public static AudioStream loadRaw(byte[] buffer, int sample_rate = 11025)
    {
        var sample = new AudioStreamSample();
        sample.Format = AudioStreamSample.FormatEnum.Format8Bits;
        sample.MixRate = sample_rate;
        sample.Stereo = false;
        sample.Data = buffer;
        //sample.LoopMode = loop ? AudioStreamSample.LoopModeEnum.PingPong : AudioStreamSample.LoopModeEnum.Disabled;
        return sample;
    }

    public static AudioStream loadRaw(string path, int sample_rate = 11025)
    {
        var sample = loadRaw(loadFile(path), sample_rate);
        return sample;
    }

    public static byte[] loadFile(string path)
    {
        var f = new File();
        f.Open(path, File.ModeFlags.Read); // case insensitive search for Unix FSs is impossible now
        var buffer = f.GetBuffer((int)f.GetLen());
        f.Close();
        return buffer;
    }

    public static AudioStream loadOGG(byte[] buffer, bool loop = false)
    {
        var stream = new AudioStreamOGGVorbis();
        stream.Data = buffer;
        stream.Loop = loop;

        return stream;
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
            it.CreateFromImage(image);
            it.Flags &= ~(uint)Texture.FlagsEnum.Filter;
            texture = it;
        }

        return texture;
    }

    public static TileSet makeTileset(Texture texture, bool collisions)
    {
        BitMap bitmap = null;
        if (collisions)
        {
            var image = texture.GetData();
            bitmap = new BitMap();
            bitmap.CreateFromImageAlpha(image, .01f);
        }

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

                if (collisions)
                {
                    var polygons = bitmap.OpaqueToPolygons(region, 2);
                    int c = 0;

                    for (int j = 0; j < polygons.Count; j++)
                    {
                        var collision = new ConvexPolygonShape2D();
                        Vector2[] v = (Vector2[])polygons[j];
                        List<Vector2> plist = new List<Vector2>();
                        for (int k = 0; k < v.Length; k++)
                        {
                            // I have no idea why it's adding y*48 to y coordinates...
                            Vector2 mv = new Vector2(v[k].x, v[k].y - (y * TILE_HEIGHT * 2));
                            plist.Add(mv);
                        }

                        if (plist.Count < 3) { continue; }
                        collision.SetPointCloud(plist.ToArray());
                        ts.TileSetShape(i, c++, collision);
                    }
                }
                i++;
            }
        }
        return ts;
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

    // Call this in any level-optimizing procedure (level load screen, post-download processing, special button). Currently disabled.
    public static void compileInternalTileset(KnyttWorld world, bool recompile)
    {
        ensureDirExists($"user://Cache/{world.WorldDirectoryName}");

        for (int num = 0; num < 256; num++)
        {
            string tileset_path = $"Tilesets/Tileset{num}.png";
            if (!world.worldFileExists(tileset_path)) { continue; }

            string cached_path = $"user://Cache/{world.WorldDirectoryName}/Tileset{num}.res";
            if (!recompile && new File().FileExists(cached_path)) { continue; }

            var texture = world.getWorldTexture(tileset_path);
            if (texture is Texture t)
            {
                t = preprocessTilesetTexture(t);
                ResourceSaver.Save(cached_path, makeTileset(t, true), ResourceSaver.SaverFlags.Compress);
            }
        }
    }

    // Create "user://tilesets" directory and call this function at start to compile default tilesets
    public static void compileTileset()
    {
        for (int i = 0; i < 256; i++)
        {
            GD.Print($"Compiling tileset #{i}");
            var texture = loadInternalTexture($"res://knytt/data/Tilesets/Tileset{i}.png");
            var tileset = makeTileset(texture, true);
            GD.PrintErr(ResourceSaver.Save($"user://tilesets/Tileset{i}.png.res", tileset, ResourceSaver.SaverFlags.Compress));
        }
    }
}
