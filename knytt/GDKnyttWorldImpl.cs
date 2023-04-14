using Godot;
using YKnyttLib;

public partial class GDKnyttWorldImpl : KnyttWorld
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
        return FileAccess.FileExists(full_path);
    }

    protected override object getExternalSound(string filepath, bool loop)
    {
        var full_path = this.WorldDirectory + "/" + filepath.ToLower();
        if (!FileAccess.FileExists(full_path)) { return null; }
        return GDKnyttAssetManager.loadExternalSound(full_path, loop);
    }

    protected override object getExternalTexture(string filepath)
    {
        var full_path = this.WorldDirectory + "/" + filepath.ToLower();
        if (!FileAccess.FileExists(full_path)) { return null; }
        return GDKnyttAssetManager.loadExternalTexture(full_path);
    }

    protected sealed override byte[] getExternalWorldData(string filepath)
    {
        var full_path = this.WorldDirectory + "/" + filepath.ToLower();
        if (!FileAccess.FileExists(full_path)) { return null; }
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
        using var marker = FileAccess.Open(marker_name, FileAccess.ModeFlags.Write);
        marker.Close();

        foreach (string filename in BinLoader.GetFileNames())
        {
            string fullname = $"{dir}/{filename}";
            GDKnyttAssetManager.ensureDirExists(fullname.Substring(0, fullname.LastIndexOf('/')));

            using var f = FileAccess.Open(fullname, FileAccess.ModeFlags.Write);
            f.StoreBuffer(BinLoader.GetFile(filename));
        }

        DirAccess.RemoveAbsolute(marker_name);
        DirAccess.RemoveAbsolute(WorldDirectory);
        removeDirectory("user://Cache/" + GDKnyttAssetManager.extractFilename(WorldDirectory));

        purgeBinFile();
        setDirectory(dir, WorldDirectoryName);
        BinMode = false;
    }

    private void removeDirectory(string dir_name)
    {
        using var dir = DirAccess.Open(dir_name);
        if (!dir.DirExists(dir_name)) { return; }
        dir.IncludeNavigational = false;
        dir.ListDirBegin();
        for (string filename = dir.GetNext(); filename != ""; filename = dir.GetNext())
        {
            if (dir.FileExists(filename)) { dir.Remove(filename); } else { removeDirectory($"{dir_name}/{filename}"); }
        }
        dir.ListDirEnd();
        dir.Remove(dir_name);
    }

    public void uninstallWorld()
    {
        if (BinMode)
        {
            DirAccess.RemoveAbsolute(WorldDirectory);
            removeDirectory("user://Cache/" + GDKnyttAssetManager.extractFilename(WorldDirectory));
        }
        else
        {
            removeDirectory(WorldDirectory);
        }
        removeDirectory($"user://Cache/{WorldDirectoryName}");
        // TODO: also remove .part files which might left after an unfinished download
    }
}
