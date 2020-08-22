using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;
using YUtil.Collections;

public class GDKnyttAssetManager
{
    public GDKnyttWorld GDWorld { get; }
    private Dictionary<string, string> Directories { get; }

    ObjectCache<int, TileSet> TileSetCache;
    ObjectCache<int, Texture> GradientCache;
    ObjectCache<int, AudioStream> SongCache;
    ObjectCache<int, AudioStream> AmbianceCache;
    
    public GDKnyttAssetManager(GDKnyttWorld gdworld, int tile_size, int gradient_size, int song_size, int ambiance_size)
    {
        this.GDWorld = gdworld;
        Directories = new Dictionary<string, string>();

        TileSetCache = new ObjectCache<int, TileSet>(tile_size);
        TileSetCache.OnCreate = (int num) => buildTileSet(num);

        GradientCache = new ObjectCache<int, Texture>(gradient_size);
        GradientCache.OnCreate = (int num) => buildGradient(num);

        SongCache = new ObjectCache<int, AudioStream>(song_size);
        SongCache.OnCreate = (int num) => buildSong(num);

        AmbianceCache = new ObjectCache<int, AudioStream>(ambiance_size);
        AmbianceCache.OnCreate = (int num) => buildAmbiance(num);
    }

    public TileSet getTileSet(int num)
    {
        return TileSetCache.IncObject(num);
    }

    public void returnTileSet(int num)
    {
        TileSetCache.DecObject(num);
    }

    public Texture getGradient(int num)
    {
        return GradientCache.IncObject(num);
    }

    public void returnGradient(int num)
    {
        GradientCache.DecObject(num);
    }

    public AudioStream getSong(int num)
    {
        return SongCache.IncObject(num);
    }

    public void returnSong(int num)
    {
        SongCache.DecObject(num);
    }

    public AudioStream getAmbiance(int num)
    {
        return AmbianceCache.IncObject(num);
    }

    public void returnAmbiance(int num)
    {
        AmbianceCache.DecObject(num);
    }

    private TileSet buildTileSet(int num)
    {
        //GD.Print("Building TileSet: " + num);
        string fname = this.GDWorld.World.TilesetsOverride[num];
        if (fname == null) { fname = string.Format("res://knytt/data/Tilesets/Tileset{0}.png", num); }
        var texture = loadTexture(fname);

        return GDKnyttAssetBuilder.makeTileset(texture, false);
    }

    private Texture buildGradient(int num)
    {
        string fname = this.GDWorld.World.TilesetsOverride[num];
        if (fname == null) { fname = string.Format("res://knytt/data/Gradients/Gradient{0}.png", num); }

        return loadTexture(fname);
    }

    public AudioStream buildSong(int num)
    {
        if (num == 0) { return null; }
        string fname = this.GDWorld.World.MusicOverride[num];
        if (fname == null) { fname = string.Format("res://knytt/data/Music/Song{0}.ogg", num); }
        var f = new File();
        f.Open(fname, File.ModeFlags.Read);
        var buffer = f.GetBuffer((int)f.GetLen());
        f.Close();

        //var stream = new AudioStreamSample(); // This is for wav
        var stream = new AudioStreamOGGVorbis();
        stream.Data = buffer;
        stream.Loop = false;

        return stream;
    }

    public AudioStream buildAmbiance(int num)
    {
        if (num == 0) { return null; }
        string fname = this.GDWorld.World.AmbianceOverride[num];
        if (fname == null) { fname = string.Format("res://knytt/data/Ambiance/Ambi{0}.ogg", num); }
        var f = new File();
        f.Open(fname, File.ModeFlags.Read);
        var buffer = f.GetBuffer((int)f.GetLen());
        f.Close();

        var stream = new AudioStreamOGGVorbis();
        stream.Data = buffer;
        stream.Loop = true;

        return stream;
    }

    public void discoverWorldStructure(Directory world_dir)
    {
        world_dir.ListDirBegin();
        while(true)
        {
            string name = world_dir.GetNext();
            if (name.Length == 0) { break; }
            string lname = name.ToLower();
            if (world_dir.CurrentIsDir())
            {
                var dname = world_dir.GetCurrentDir() + "/" + name;
                this.Directories.Add(lname, dname);
                if (lname.Equals("tilesets")) { checkAssetOverrides(dname, this.GDWorld.World.TilesetsOverride); }
                else if (lname.Equals("ambiance")) { checkAssetOverrides(dname, this.GDWorld.World.AmbianceOverride); }
                else if (lname.Equals("gradients")) { checkAssetOverrides(dname, this.GDWorld.World.GradientsOverride); }
                else if (lname.Equals("music")) { checkAssetOverrides(dname, this.GDWorld.World.MusicOverride); }
            }
            else if (name.ToLower().Equals("map.bin")) { this.GDWorld.MapPath = world_dir.GetCurrentDir() + "/" + name; }
            else if (name.ToLower().Equals("world.ini")) { this.GDWorld.WorldConfigPath = world_dir.GetCurrentDir() + "/" + name; }
        }
        world_dir.ListDirEnd();
    }

    private void checkAssetOverrides(string dname, string[] overrides)
    {
        var asset_dir = new Directory();
        asset_dir.Open(dname);
        asset_dir.ListDirBegin();

        while(true)
        {
            string name = asset_dir.GetNext();
            if (name.Length == 0) { break; }
            if (asset_dir.CurrentIsDir()) { continue; }
            var num = Regex.Match(name, @"\d+").Value;
            //GD.Print("'" + name + "' " + num);
            this.GDWorld.World.TilesetsOverride[int.Parse(num)] = asset_dir.GetCurrentDir() + "/" + name;
        }

        asset_dir.ListDirEnd();
    }

    public static Texture loadTexture(string path)
    {
        var image = new Image();
        image.Load(path);
        var texture = new ImageTexture();
        texture.CreateFromImage(image, (int)Texture.FlagsEnum.Repeat);
        return texture;
    }

    public static string loadTextFile(string path)
    {
        var f = new File();
        f.Open(path, File.ModeFlags.Read);
        string txt = f.GetAsText();
        f.Close();
        return txt;
    }

}
