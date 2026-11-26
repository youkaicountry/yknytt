using Godot;
using System.Collections.Generic;
using YKnyttLib;

// Where the level's files are, for KnyttCheck: in the level, or in the game's data. Answers are kept, so ask a new
// one when files may have changed
public class WorldFiles : KnyttFiles
{
    private readonly KnyttWorld world;
    private readonly Dictionary<string, bool> in_level = new Dictionary<string, bool>();
    private readonly Dictionary<string, bool> in_game = new Dictionary<string, bool>();
    private readonly Dictionary<string, (int, int)?> sizes = new Dictionary<string, (int, int)?>();

    public WorldFiles(KnyttWorld world)
    {
        this.world = world;
    }

    public bool level(string path)
    {
        if (!in_level.ContainsKey(path)) { in_level[path] = world.worldFileExists(path); }
        return in_level[path];
    }

    public bool game(string path)
    {
        if (!in_game.ContainsKey(path)) { in_game[path] = ResourceLoader.Exists($"res://knytt/data/{path}"); }
        return in_game[path];
    }

    public (int width, int height)? size(string path)
    {
        if (!sizes.ContainsKey(path)) { sizes[path] = KnyttCheck.pngSize(world.getWorldData(path)); }
        return sizes[path];
    }
}
