using Godot;
using YKnyttLib;

public class GDKnyttWorldImpl : KnyttWorld
{
    public GDKnyttWorldImpl() : base() { }

    protected override object bytesToSound(byte[] data, bool loop)
    {
        return GDKnyttAssetManager.loadOGG(data, loop);
    }

    protected override object bytesToTexture(byte[] data)
    {
        return GDKnyttAssetManager.loadTexture(data);
    }

    protected override bool externalFileExists(string filepath)
    {
        var full_path = this.WorldDirectory + "/" + filepath.ToLower();
        var f = new File();
        return f.FileExists(full_path);
    }

    protected override object getExternalSound(string filepath, bool loop)
    {
        var full_path = this.WorldDirectory + "/" + filepath.ToLower();
        var f = new File();
        if (!f.FileExists(full_path)) { return null; }
        return GDKnyttAssetManager.loadExternalSound(full_path, loop);
    }

    protected override object getExternalTexture(string filepath)
    {
        var full_path = this.WorldDirectory + "/" + filepath.ToLower();
        var f = new File();
        if (!f.FileExists(full_path)) { return null; }
        return GDKnyttAssetManager.loadExternalTexture(full_path);
    }

    protected sealed override byte[] getExternalWorldData(string filepath)
    {
        var full_path = this.WorldDirectory + "/" + filepath.ToLower();
        var f = new File();
        if (!f.FileExists(full_path)) { return null; }
        return GDKnyttAssetManager.loadFile(full_path);
    }

    protected override object getSystemSound(string filepath, bool loop)
    {
        return GDKnyttAssetManager.loadInternalSound("res://knytt/data/" + filepath, loop);
    }

    protected override object getSystemTexture(string filepath)
    {
        return (object)GDKnyttAssetManager.loadInternalTileset($"res://knytt/data/Compiled/{filepath}.res") ??
            GDKnyttAssetManager.loadInternalTexture("res://knytt/data/" + filepath);
    }

    protected sealed override byte[] getSystemWorldData(string filepath)
    {
        return GDKnyttAssetManager.loadFile("res://knytt/data/" + filepath);
    }

    // Call this if you don't want a level placed in RAM
    public void unpackWorld()
    {
        string dir = $"user://Worlds/{BinLoader.RootDirectory}";
        GDKnyttAssetManager.ensureDirExists(dir);

        string marker_name = $"{dir}/_do_not_load_";
        File marker = new File();
        marker.Open(marker_name, File.ModeFlags.Write);
        marker.Close();

        foreach (string filename in BinLoader.GetFileNames())
        {
            string fullname = $"{dir}/{filename}";
            GDKnyttAssetManager.ensureDirExists(fullname.Substring(0, fullname.LastIndexOf('/')));

            File f = new File();
            f.Open(fullname, File.ModeFlags.Write);
            f.StoreBuffer(BinLoader.GetFile(filename));
            f.Close();
        }

        new Directory().Remove(marker_name);
        new Directory().Remove(WorldDirectory);
        removeDirectory("user://Cache/" + WorldDirectory.Substring(WorldDirectory.LastIndexOfAny("/\\".ToCharArray()) + 1));

        purgeBinFile();
        setDirectory(dir, WorldDirectoryName);
        BinMode = false;
    }

    private void removeDirectory(string dir_name)
    {
        var dir = new Directory();
        dir.Open(dir_name);
        dir.ListDirBegin(skipNavigational: true);
        for (string filename = dir.GetNext(); filename != ""; filename = dir.GetNext())
        {
            if (dir.FileExists(filename)) { dir.Remove(filename); } else { removeDirectory($"{dir_name}/{filename}"); }
        }
        new Directory().Remove(dir_name);
    }

    public void uninstallWorld()
    {
        if (BinMode)
        {
            new Directory().Remove(WorldDirectory);
            removeDirectory("user://Cache/" + WorldDirectory.Substring(WorldDirectory.LastIndexOfAny("/\\".ToCharArray()) + 1));
        }
        else
        {
            removeDirectory(WorldDirectory);
        }
        removeDirectory($"user://Cache/{WorldDirectoryName}");
        // TODO: also remove .part files which might left after an unfinished download
    }
}
