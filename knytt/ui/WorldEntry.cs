using Godot;
using System;
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
    public long FileSize;
    public int Upvotes;
    public int Downvotes;
    public int Downloads;
    public int Complains;
    public bool AutoVerified;
    public int Status;
    public bool Disabled;
    public int Completed;
    public bool HasSaves;
    public float OverallScore;
    public int Voters;
    public int UserScore; // be careful! 2x scale - 0..10
    public int[] Completions = new int[7];

    public WorldEntry() { }

    public WorldEntry(KnyttWorldInfo world_info, string path)
    {
        Name = world_info.Name;
        Author = world_info.Author;
        Description = world_info.Description;
        Size = world_info.Size;
        Difficulties = world_info.Difficulties;
        Categories = world_info.Categories;
        HasServerInfo = false;
        Path = path;
        Completed = world_info.Completed;
        UserScore = world_info.Score;

        FileSize = world_info.FileSize;
        if (FileSize != 0 || new Directory().DirExists(path))
        {
            InstalledTime = new File().GetModifiedTime(path);
        }
        else
        {
            try
            {
                if (path.StartsWith("user://")) { path = OS.GetUserDataDir().PlusFile(path.Substring(7)); }
                var file_info = new System.IO.FileInfo(path);
                InstalledTime = (ulong)((DateTimeOffset)file_info.LastWriteTimeUtc).ToUnixTimeSeconds();
                FileSize = file_info.Length;
            }
            catch (Exception)
            {
                InstalledTime = new File().GetModifiedTime(path);
            }
        }
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
        UserScore = info.UserScore;
    }

    private static string[] statuses = {"Not Verified", "Unfinished", "Broken", 
        "Almost Broken", "Partially Playable", "Almost Playable", "Playable"};

    public string StatusDescription => Status == 0 && AutoVerified ? "Auto-verified" : 
                                       Status >= statuses.Length ? "Undefined" : statuses[Status];

    private static Color[] status_colors = {new Color(0.5f, 0, 0), new Color(0.5f, 0, 0), new Color(0.5f, 0, 0), 
        new Color(0.5f, 0, 0), new Color(0, 0, 0.5f), new Color(0, 0, 0.5f), new Color(0, 0.5f, 0)};

    public Color StatusColor => Status == 0 && AutoVerified ? new Color(0, 0.5f, 0) : 
                                Status >= status_colors.Length ? new Color(0, 0, 0) : status_colors[Status];

    public Color ScoreColor => Voters == 0 ? new Color(0.5f, 0.5f, 0.5f) :
                               OverallScore > 3.4f ? new Color(0, 0.5f, 0) : new Color(0.5f, 0, 0);

    private static Color[] completion_colors = {new Color(0.75f, 0.75f, 0.75f), new Color(1f, 0.95f, 0.65f), 
        new Color(0.9f, 0.65f, 0.55f), new Color(0.9f, 0.75f, 1f), new Color(1f, 0.75f, 0.75f), new Color(0.75f, 0.5f, 0.5f)};

    public Color CompletionColor => Completed > 0 ? completion_colors[Completed - 1] :
        (HasSaves ? new Color(0.85f, 1f, 0.85f) : new Color(1, 1, 1));
}
