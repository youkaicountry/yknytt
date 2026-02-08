using Godot;
using YKnyttLib;

public partial class GDKnyttWorldImpl : KnyttWorld
{
    public GDKnyttWorldImpl() : base() { }

    protected override object bytesToSound(byte[] data, bool loop)
    {
        return GDKnyttAssetManager.loadOGG(data, loop);
    }

    protected override object bytesToTexture2D(byte[] data)
    {
        return GDKnyttAssetManager.loadTexture2D(data);
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

    protected override object getExternalTexture2D(string filepath)
    {
        var full_path = this.WorldDirectory + "/" + filepath.ToLower();
        if (!FileAccess.FileExists(full_path)) { return null; }
        return GDKnyttAssetManager.loadExternalTexture2D(full_path);
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

    protected override object getSystemTexture2D(string filepath)
    {
        // Skip pre-compiled Godot 3 TileSet .res files - they are incompatible with Godot 4.
        // makeTileset() will create proper Godot 4 TileSets from the PNG textures instead.
        return GDKnyttAssetManager.loadInternalTexture2D("res://knytt/data/" + filepath);
    }

    protected sealed override byte[] getSystemWorldData(string filepath)
    {
        return GDKnyttAssetManager.loadFile("res://knytt/data/" + filepath);
    }

    // Call this if you don't want a level placed in RAM
    public void unpackWorld(string dir)
    {
        GDKnyttDataStore.ProgressHint = "Starting unpacking .bin file...";
        GDKnyttAssetManager.ensureDirExists(dir);

        string marker_name = $"{dir}/_do_not_load_";
        using (var marker = FileAccess.Open(marker_name, FileAccess.ModeFlags.Write)) { }

        foreach (string filename in BinLoader.GetFileNames())
        {
            GDKnyttDataStore.ProgressHint = $"Unpacking {filename}...";
            string fullname = $"{dir}/{filename}";
            GDKnyttAssetManager.ensureDirExists(fullname.Substring(0, fullname.LastIndexOf('/')));

            using var f = FileAccess.Open(fullname, FileAccess.ModeFlags.Write);
            f.StoreBuffer(BinLoader.GetFile(filename));
        }

        GDKnyttDataStore.ProgressHint = "Finishing unpacking...";
        DirAccess.RemoveAbsolute(marker_name);
        DirAccess.RemoveAbsolute(WorldDirectory);

        purgeBinFile();
        setDirectory(dir, WorldDirectoryName);
        BinMode = false;
        GDKnyttDataStore.ProgressHint = "Unpacking completed.";
    }

    public static void removeDirectory(string dir_name)
    {
        if (!DirAccess.DirExistsAbsolute(dir_name)) { return; }
        using var dir = DirAccess.Open(dir_name);
        if (dir == null) { return; }
        dir.ListDirBegin();
        for (string filename = dir.GetNext(); filename != ""; filename = dir.GetNext())
        {
            if (filename == "." || filename == "..") { continue; }
            if (dir.FileExists(filename)) { dir.Remove(filename); } else { removeDirectory($"{dir_name}/{filename}"); }
        }
        dir.ListDirEnd();
        DirAccess.RemoveAbsolute(dir_name);
    }

    public void uninstallWorld()
    {
        if (BinMode)
        {
            DirAccess.RemoveAbsolute(WorldDirectory);
        }
        else
        {
            removeDirectory(WorldDirectory);
        }
        removeDirectory(GDKnyttDataStore.BaseDataDirectory.PathJoin("Cache").PathJoin(WorldDirectoryName));
    }

    public void refreshWorld()
    {
        if (BinMode)
        {
            BinLoader = new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile(WorldDirectory));
        }
        removeDirectory(GDKnyttDataStore.BaseDataDirectory.PathJoin("Cache").PathJoin(WorldDirectoryName));
    }
}
