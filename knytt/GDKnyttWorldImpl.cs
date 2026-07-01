using System;
using Godot;
using YKnyttLib;

public class GDKnyttWorldImpl : KnyttWorld
{
    public int Completed { get; set; }
    public int UserScore { get; set; }

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

    public Texture loadOverflownTexture(string filepath, int frame_width, int frame_height)
    {
        int size_limit = OS.GetName() == "Unix" ? 8192 : 16384;

        filepath = filepath.ToLower();
        //string cached_path = 
        //    GDKnyttDataStore.BaseDataDirectory.PlusFile("Cache").PlusFile(WorldDirectoryName)
        //    .PlusFile(filepath.Replace('/', '_').Replace('\\', '_'));
        //if (new File().FileExists(cached_path)) { return GDKnyttAssetManager.loadExternalTexture(cached_path); }

        string full_external_path = WorldDirectory + "/" + filepath;
        byte[] header = BinMode ? BinLoader.GetFile(filepath) : GDKnyttAssetManager.loadFile(full_external_path, 24);
        
        if (header == null || !MiniPng.TryReadSize(header, out int image_width, out int image_height)) { return null; }
        
        if (frame_width <= 0 || frame_height <= 0) { return null; }
        if (image_width < frame_width || image_height < frame_height) { return null; }
        if (size_limit < frame_width || size_limit < frame_height) { return null; }

        if (image_width <= size_limit && image_height <= size_limit) { return null; }
        if (image_width > size_limit && image_height > size_limit) { return null; }
        // if limit is not set, time for repacking is accelerating. current limit is about 2 seconds for cheap Linux handhelds
        if ((long)image_width * image_height > Juni.OVERFLOWN_CO_LIMIT_PX) { return null; }

        byte[] full = BinMode ? BinLoader.GetFile(filepath) : GDKnyttAssetManager.loadFile(full_external_path);
        if (full == null) { return null; }

        Image repacked = repackToFit(full, size_limit, frame_width, frame_height);
        if (repacked == null) { return null; }
        //repacked.SavePng(cached_path);

        var texture = new ImageTexture();
        texture.CreateFromImage(repacked, 0);
        return texture;
    }
    
    private static Image repackToFit(byte[] png_bytes, int size_limit, int frame_width, int frame_height)
    {
        if (png_bytes == null || png_bytes.Length == 0) { return null; }

        if (!MiniPng.TryDecode(png_bytes, out int source_width, out int source_height, out byte[] src_rgba)) { return null; }

        int src_cols = source_width / frame_width;
        int src_rows = source_height / frame_height;

        int total_frames = src_cols * src_rows;

        int max_cols = size_limit / frame_width;
        int max_rows = size_limit / frame_height;

        int new_cols = Math.Min(max_cols, total_frames);
        int new_rows = (total_frames + new_cols - 1) / new_cols;
        new_cols = (total_frames + new_rows - 1) / new_rows;

        if (new_cols > max_cols || new_rows > max_rows)
            return null;

        int new_width = new_cols * frame_width;
        int new_height = new_rows * frame_height;

        int src_stride = source_width * 4;
        int dst_stride = new_width * 4;
        byte[] dst_rgba = new byte[new_width * new_height * 4];

        int index = 0;
        while (index < total_frames)
        {
            int src_col = index % src_cols;
            int src_row = index / src_cols;
            int dst_col = index % new_cols;
            int dst_row = index / new_cols;

            int run_len = Math.Min(src_cols - src_col, new_cols - dst_col);

            int copy_w = run_len * frame_width * 4;
            int src_x = src_col * frame_width * 4;
            int dst_x = dst_col * frame_width * 4;

            for (int local_y = 0; local_y < frame_height; local_y++)
            {
                int src_off = (src_row * frame_height + local_y) * src_stride + src_x;
                int dst_off = (dst_row * frame_height + local_y) * dst_stride + dst_x;
                Array.Copy(src_rgba, src_off, dst_rgba, dst_off, copy_w);
            }

            index += run_len;
        }

        var output_image = new Image();
        output_image.CreateFromData(new_width, new_height, false, Image.Format.Rgba8, dst_rgba);
        return output_image;
    }

    // Call this if you don't want a level placed in RAM
    public void unpackWorld(string dir)
    {
        GDKnyttDataStore.ProgressHint = "Starting unpacking .bin file...";
        GDKnyttAssetManager.ensureDirExists(dir);

        string marker_name = $"{dir}/_do_not_load_";
        File marker = new File();
        marker.Open(marker_name, File.ModeFlags.Write);
        marker.Close();

        foreach (string filename in BinLoader.GetFileNames())
        {
            GDKnyttDataStore.ProgressHint = $"Unpacking {filename}...";
            string fullname = $"{dir}/{filename}";
            GDKnyttAssetManager.ensureDirExists(fullname.Substring(0, fullname.LastIndexOf('/')));

            File f = new File();
            f.Open(fullname, File.ModeFlags.Write);
            f.StoreBuffer(BinLoader.GetFile(filename));
            f.Close();
        }

        GDKnyttDataStore.ProgressHint = "Finishing unpacking...";
        new Directory().Remove(marker_name);
        new Directory().Remove(WorldDirectory);

        purgeBinFile();
        setDirectory(dir, WorldDirectoryName);
        BinMode = false;
        GDKnyttDataStore.ProgressHint = "Unpacking completed.";
    }

    public static void removeDirectory(string dir_name, string prefix = "")
    {
        var dir = new Directory();
        if (!dir.DirExists(dir_name)) { return; }
        dir.Open(dir_name);
        dir.ListDirBegin(skipNavigational: true);
        for (string filename = dir.GetNext(); filename != ""; filename = dir.GetNext())
        {
            if (!filename.StartsWith(prefix)) { continue; }
            if (dir.FileExists(filename)) { dir.Remove(filename); } else { removeDirectory($"{dir_name}/{filename}"); }
        }
        dir.ListDirEnd();
        new Directory().Remove(dir_name);
    }

    public void uninstallWorld()
    {
        if (BinMode)
        {
            new Directory().Remove(WorldDirectory);
        }
        else
        {
            removeDirectory(WorldDirectory);
        }
        removeDirectory(GDKnyttDataStore.BaseDataDirectory.PlusFile("Cache").PlusFile(WorldDirectoryName));
    }

    public void refreshWorld(bool tilesets_only)
    {
        if (BinMode)
        {
            BinLoader = new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile(WorldDirectory));
        }
        loadWorldConfig(GDKnyttAssetManager.loadTextFileRaw(getWorldData("World.ini")));
        removeDirectory(GDKnyttDataStore.BaseDataDirectory.PlusFile("Cache").PlusFile(WorldDirectoryName), 
                        prefix: tilesets_only ? "qTileset" : "");
    }

    public void removeOldTilesets()
    {
        string cache_dir = GDKnyttDataStore.BaseDataDirectory.PlusFile("Cache").PlusFile(WorldDirectoryName);
        removeDirectory(cache_dir, prefix: "Tileset");

        // There was a bug which compiled and saved default tilesets in cache directory
        var dir = new Directory();
        dir.Open(cache_dir);
        dir.ListDirBegin(skipNavigational: true);
        for (string filename = dir.GetNext(); filename != ""; filename = dir.GetNext())
        {
            if (filename.StartsWith("qTileset"))
            {
                string tileset_name = filename.Substring(1, filename.Length - 5);
                if (!worldFileExists($"Tilesets/{tileset_name}.png")) { dir.Remove(filename); }
            }
        }
    }
}
