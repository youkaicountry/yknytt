using Godot;
using YKnyttLib;

public class LevelSelection : CanvasLayer
{
    KnyttWorldManager<Texture> Manager { get; }
    PackedScene info_scene;

    public LevelSelection()
    {
        Manager = new KnyttWorldManager<Texture>();
    }

    public override void _Ready()
    {
        this.info_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/InfoScreen.tscn");
        this.discoverWorlds("./worlds");
        this.listWorlds();
    }

    // Search the given directory for worlds
    // TODO: This should do a very different process for bin files
    private void discoverWorlds(string path)
    {
        var wd = new Directory();
        wd.Open(path);

        wd.ListDirBegin();
        while(true)
        {
            string name = wd.GetNext();
            if (name.Length == 0) { break; }
            
            if (!verifyWorld(wd, name)) { continue; }
            var icon = GDKnyttAssetManager.loadTexture(wd.GetCurrentDir() + "/" + name + "/Icon.png");
            var txt = GDKnyttAssetManager.loadTextFile(wd.GetCurrentDir() + "/" + name + "/World.ini");
            GDKnyttWorldImpl world = new GDKnyttWorldImpl();
            world.setDirectory(wd.GetCurrentDir() + "/" + name, name);
            world.loadWorldConfig(txt);
            Manager.addWorld(world, icon);
        }
        wd.ListDirEnd();
    }

    private bool verifyWorld(Directory dir, string name)
    {
        if (!dir.CurrentIsDir() || name.Equals(".") || name.Equals("..")) { return false; }

        // TODO: Check for file signatures
        return true;
    }

    private void listWorlds()
    {
        // Should take filter settings into account
        var worlds = Manager.filter(null, null, null);
        var game_container = GetNode<GameContainer>("MainContainer/ScrollContainer/GameContainer");
        game_container.clearWorlds();

        foreach(var world_entry in worlds)
        {
            game_container.addWorld((GDKnyttWorldImpl)world_entry.world, world_entry.extra_data);
        }
    }

    public void _on_BackButton_pressed()
    {
        GetNodeOrNull<AudioStreamPlayer>("../MenuClickPlayer")?.Play();
        this.QueueFree();
    }

    public void _on_GamePressed(GameButton button)
    {
        GetNodeOrNull<AudioStreamPlayer>("../MenuClickPlayer")?.Play();
        var info = info_scene.Instance() as InfoScreen;
        info.initialize(button.World);
        this.GetParent().AddChild(info);
    }
}
