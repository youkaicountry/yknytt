using Godot;
using YKnyttLib;

public class GDKnyttWorldImpl : KnyttWorld
{
    public GDKnyttWorldImpl() : base() { }

    protected sealed override byte[] getExternalWorldFile(string filepath)
    {
        var full_path = this.WorldDirectory + "/" + filepath;
        var f = new File();
        if (!f.FileExists(full_path)) { return null; }
        return GDKnyttAssetManager.loadFile(full_path);
    }

    protected sealed override byte[] getSystemWorldFile(string filepath)
    {
        return GDKnyttAssetManager.loadFile("res://knytt/data/" + filepath);
    }
}
