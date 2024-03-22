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
    public ulong InstalledTime;
    public ulong LastPlayedTime;
    public string Path;
    public bool HasServerInfo;
    public string Link;
    public int FileSize;
    public int Upvotes;
    public int Downvotes;
    public int Downloads;
    public int Complains;
    public int Completions;
    public bool AutoVerified;
    public int Status;
    public bool Disabled;
    public bool Completed;

    public WorldEntry() { }

    public WorldEntry(Texture icon, KnyttWorldInfo world_info, string path, ulong last_played, bool completed)
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
        InstalledTime = new File().GetModifiedTime(path);
        LastPlayedTime = last_played;
        Completed = completed;
    }

    public void MergeLocal(WorldEntry info)
    {
        Size = info.Size;
        Difficulties = info.Difficulties;
        Categories = info.Categories;
        Path = info.Path;
        InstalledTime = info.InstalledTime;
        LastPlayedTime = info.LastPlayedTime;
        Completed = info.Completed;
    }

    private static string[] statuses = {"Not Verified", "Unfinished", "Broken", 
        "Almost Broken", "Partially Playable", "Almost Playable", "Playable"};

    public string StatusDescription => Status == 0 && AutoVerified ? "Auto-verified" : 
                                       Status >= statuses.Length ? "Undefined" : statuses[Status];

    private static Color[] status_colors = {new Color(0.5f, 0, 0), new Color(0.5f, 0, 0), new Color(0.5f, 0, 0), 
        new Color(0.5f, 0, 0), new Color(0, 0, 0.5f), new Color(0, 0, 0.5f), new Color(0, 0.5f, 0)};

    public Color StatusColor => Status == 0 && AutoVerified ? new Color(0, 0.5f, 0) : 
                                Status >= status_colors.Length ? new Color(0, 0, 0) : status_colors[Status];
}
