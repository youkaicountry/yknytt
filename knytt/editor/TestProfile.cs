using Godot;
using IniParser.Model;
using IniParser.Parser;
using System.Collections.Generic;
using System.Linq;
using YKnyttLib;

// A start state for tests: a savegame holding only what it sets, laid over the level's own.
// Kept on this machine beside the level, never inside it
public class TestProfile
{
    public string Name { get; private set; }
    public KnyttSave Save { get; }

    private static readonly string[] STATISTICS =
        { "Total Deaths", "Total Time", "Hardest Place", "Hardest Place Deaths", "Visited Areas", "Gzipped Visited Areas" };

    private readonly KnyttWorld world;

    private TestProfile(KnyttWorld world, string name, string ini)
    {
        this.world = world;
        Name = name;
        Save = new KnyttSave(world, ini, 0);
    }

    public static string folder(KnyttWorld world) =>
        GDKnyttDataStore.BaseDataDirectory.PlusFile("Editor").PlusFile(world.WorldDirectoryName).PlusFile("Tests");

    private string FileName => folder(world).PlusFile($"{Name}.ini");

    public static List<string> names(KnyttWorld world)
    {
        var names = new List<string>();
        var dir = new Directory();
        if (dir.Open(folder(world)) != Error.Ok) { return names; }
        dir.ListDirBegin(skipNavigational: true);
        for (string file = dir.GetNext(); file != ""; file = dir.GetNext())
        {
            if (file.EndsWith(".ini")) { names.Add(file.BaseName()); }
        }
        dir.ListDirEnd();
        names.Sort();
        return names;
    }

    public static TestProfile load(KnyttWorld world, string name)
    {
        var data = GDKnyttAssetManager.loadFile(folder(world).PlusFile($"{name}.ini"));
        return data == null ? null : new TestProfile(world, name, GDKnyttAssetManager.loadTextFile(data));
    }

    // The level's own start, read only
    public static TestProfile levelDefault(KnyttWorld world)
    {
        var data = world.getWorldData("DefaultSavegame.ini");
        return new TestProfile(world, "Level default", data == null ? "" : GDKnyttAssetManager.loadTextFile(data));
    }

    // What a test starts with: the level's start with the profile laid over it
    public static KnyttSave startSave(KnyttWorld world, TestProfile profile)
    {
        var save = levelDefault(world).Save;
        if (profile != null) { save.Data.Merge(profile.Save.Data); }
        if (save.Data.Sections.ContainsSection("Extras")) { save.RemoveKSMark(); }
        return save;
    }

    // A new profile takes the state of another, but not where it starts
    public static TestProfile create(KnyttWorld world, string name, KnyttSave from)
    {
        var profile = new TestProfile(world, freeName(world, name), "");
        profile.takeState(from);
        profile.save();
        return profile;
    }

    private static string freeName(KnyttWorld world, string name)
    {
        name = string.Concat(name.Where(c => !"/\\:*?\"<>|".Contains(c))).Trim();
        if (name == "") { name = "Test"; }
        var taken = names(world);
        string free = name;
        for (int i = 2; taken.Contains(free); i++) { free = $"{name} {i}"; }
        return free;
    }

    public void save()
    {
        GDKnyttAssetManager.ensureDirExists(folder(world));
        var f = new File();
        f.Open(FileName, File.ModeFlags.Write);
        f.StoreString(Save.ToString());
        f.Close();
    }

    public void rename(string name)
    {
        if (name.Trim() == Name) { return; }
        string old_name = FileName;
        Name = freeName(world, name);
        new Directory().Rename(old_name, FileName);
    }

    public void delete()
    {
        new Directory().Remove(FileName);
    }

    // Takes another save's state, keeping its own start. Play statistics are left out
    public void takeState(KnyttSave from)
    {
        var state = new KnyttSave(world, from.ToString(), 0).Data;
        state.Sections.RemoveSection("Positions");
        foreach (string key in STATISTICS) { state["Extras"]?.RemoveKey(key); }
        removeKeys((section, key) => section != "Positions");
        Save.Data.Merge(state);
    }

    // Reading a section creates it, so look for a key
    public bool HasStart => Save.Data.Sections.GetSectionData("Positions")?.Keys.ContainsKey("X Map") ?? false;

    public void setStart(KnyttPoint area, KnyttPoint position)
    {
        Save.setArea(area);
        Save.setAreaPosition(position);
    }

    public void clearStart()
    {
        Save.Data.Sections.RemoveSection("Positions");
    }

    // KnyttSave keeps old values when nothing is collected, so clear them first
    public void setCollectables(bool[] collection, int coins_spent)
    {
        removeKeys((section, key) => isCollectable(section, key));
        Save.setCollectables(collection, coins_spent);
    }

    // What the form edits. Values that scripts declare later get claimed here too
    private static bool inForm(string section, string key) =>
        section == "Powers" || section == "Flags" || section == "Positions" || section == "World" || isCollectable(section, key);

    private static bool isCollectable(string section, string key) =>
        section == "Extras" && (key.StartsWith("Creature") || key.StartsWith("Coin") || key.StartsWith("Artifact"));

    // Everything else, as INI: other extras, and whatever scripts keep in the savegame
    public string More
    {
        get
        {
            var more = new IniData();
            foreach (var section in Save.Data.Sections)
            {
                foreach (var key in section.Keys.Where(key => !inForm(section.SectionName, key.KeyName)))
                {
                    more.Sections.AddSection(section.SectionName);
                    more[section.SectionName][key.KeyName] = key.Value;
                }
            }
            return more.ToString().Trim();
        }
    }

    // False when the text isn't valid INI
    public bool setMore(string text)
    {
        IniData more;
        try { more = new IniDataParser().Parse(text); }
        catch (IniParser.Exceptions.ParsingException) { return false; }

        removeKeys((section, key) => !inForm(section, key));
        Save.Data.Merge(more);
        return true;
    }

    private void removeKeys(System.Func<string, string, bool> which)
    {
        foreach (var section in Save.Data.Sections)
        {
            foreach (var key in section.Keys.Select(key => key.KeyName).Where(key => which(section.SectionName, key)).ToList())
            {
                section.Keys.RemoveKey(key);
            }
        }
    }
}
