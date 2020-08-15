using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;
using YKnyttLib;

public class GKnyttWorld : Node2D
{
    PackedScene area_scene;

    Dictionary<string, string> directories;
    public string MapPath { get; private set; }
    public string WorldConfigPath { get; private set; }

    public KnyttWorld<string> world;

    public override void _Ready()
    {
        this.area_scene = ResourceLoader.Load("res://knytt/GKnyttArea.tscn") as PackedScene;
        this.directories = new Dictionary<string, string>();

        this.loadDemo();
    }

    private void loadDemo()
    {
        var wd = new Directory();
        var e = wd.Open("./worlds/Nifflas - The Machine");
        this.loadWorld(wd);

        this.instantiateArea(new KnyttPoint(1001, 1000));
    }

    public void instantiateArea(KnyttPoint point)
    {
        var area = this.world.getArea(point);
        var area_node = this.area_scene.Instance() as GKnyttArea;
        area_node.loadArea(this, area);
        this.AddChild(area_node);
    }

    public void loadWorld(Directory world_dir)
    {
        this.world = new KnyttWorld<string>();
        this.discoverWorldStructure(world_dir);

        var map_file = new File();
        map_file.Open(this.MapPath, File.ModeFlags.Read);
        var data = map_file.GetBuffer((int)map_file.GetLen());
        map_file.Close();

        System.IO.MemoryStream map_stream = new System.IO.MemoryStream(data);
        
        this.world.parseWorldFiles(map_stream, null);
        GD.Print(this.world.getArea(new KnyttPoint(1000, 1000)));
    }

    public TileSet GetTileSet(int num)
    {
        // TODO: Check cache first

        // Check for override before loading internal
        string fname = this.world.TilesetsOverride[num];
        if (fname == null) { fname = string.Format("res://knytt/data/Tilesets/Tileset{0}.png", num); }
        GD.Print(fname);
        var image = new Image();
        image.Load(fname);
        var texture = new ImageTexture();
        texture.CreateFromImage(image);

        return GKnyttAssetBuilder.makeTileset(texture, false);
    }

    private void discoverWorldStructure(Directory world_dir)
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
                this.directories.Add(lname, dname);
                if (lname.Equals("tilesets")) { checkAssetOverrides(dname, this.world.TilesetsOverride); }
            }
            else if (name.ToLower().Equals("map.bin")) { this.MapPath = world_dir.GetCurrentDir() + "/" + name; }
            else if (name.ToLower().Equals("world.ini")) { this.WorldConfigPath = world_dir.GetCurrentDir() + "/" + name; }
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
            this.world.TilesetsOverride[int.Parse(num)] = asset_dir.GetCurrentDir() + "/" + name;
        }

        asset_dir.ListDirEnd();
    }
}
