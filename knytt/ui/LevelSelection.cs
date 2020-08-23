using Godot;
using YKnyttLib;

public class LevelSelection : CanvasLayer
{
    KnyttWorldManager<string, Texture> Manager { get; }

    public LevelSelection()
    {
        Manager = new KnyttWorldManager<string, Texture>();
    }

    public override void _Ready()
    {
        this.discoverWorlds("./worlds");
        this.listWorlds();
    }

    // Search the given directory for worlds
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
            KnyttWorld<string> world = new KnyttWorld<string>();
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
            game_container.addWorld(world_entry.world, world_entry.extra_data);
        }
    }

    public void _on_BackButton_pressed()
    {
        GetNodeOrNull<AudioStreamPlayer>("../MenuClickPlayer")?.Play();
        this.QueueFree();
    }
}
