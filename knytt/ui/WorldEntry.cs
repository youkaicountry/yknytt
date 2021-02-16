using Godot;
using System.Collections.Generic;
using YKnyttLib;

public class WorldEntry
{
    public string Name;
    public string Author;
    public string Description;
    public Texture Icon;
    public string Size;
    public List<string> Difficulties = new List<string>();
    public List<string> Categories = new List<string>();
    public string Path;
    public bool HasServerInfo;
    public string Link;
    public int FileSize;
    public int Upvotes;
    public int Downvotes;
    public int Downloads;
    public int Complains;
    public bool Verified;
    public bool Approved;

    public WorldEntry() { }

    public WorldEntry(Texture icon, KnyttWorldInfo world_info, string path)
    {
        Icon = icon;
        Name = world_info.Name;
        Author = world_info.Author;
        Description = world_info.Description;
        Size = world_info.Size;
        Difficulties = world_info.Difficulties;
        Categories = world_info.Categories;
        HasServerInfo = false;
        Path = path;
    }

    public void MergeLocal(WorldEntry info)
    {
        Size = info.Size;
        Difficulties = info.Difficulties;
        Categories = info.Categories;
        Path = info.Path;
    }
}
