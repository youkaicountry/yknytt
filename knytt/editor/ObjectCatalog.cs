using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IniParser.Model;
using YKnyttLib;

public class ObjectCatalog
{
    private class Entry
    {
        public string name;
        public string description = "";
    }

    private static SortedDictionary<int, string> bank_names;
    private static Dictionary<KnyttPoint, Entry> entries;

    // First object of each bank that needs KS+
    private static readonly Dictionary<int, int> PLUS_OBJECTS = new Dictionary<int, int>()
    {
        [0] = 32, [1] = 25, [6] = 14, [7] = 17, [15] = 31, [16] = 17, [19] = 1, [254] = 1, [255] = 100
    };

    private readonly KnyttWorld world;

    public ObjectCatalog(KnyttWorld world)
    {
        this.world = world;
        if (entries == null) { load(GDKnyttAssetManager.loadTextFile("res://knytt/editor/objects/Objects.txt")); }
    }

    // "3 Bank name", then "3:1 Name" or "3:1-5 Name", then tab-indented description lines
    private static void load(string text)
    {
        bank_names = new SortedDictionary<int, string>();
        entries = new Dictionary<KnyttPoint, Entry>();
        Entry entry = null;
        foreach (string line in text.Split('\n'))
        {
            if (line.StartsWith("\t") && entry != null)
            {
                entry.description += (entry.description == "" ? "" : "\n") + line.Trim();
                continue;
            }
            var match = Regex.Match(line, @"^(\d+):(\d+)(?:-(\d+))? (.+)$");
            if (match.Success)
            {
                entry = new Entry() { name = match.Groups[4].Value.Trim() };
                int bank = int.Parse(match.Groups[1].Value);
                int first = int.Parse(match.Groups[2].Value);
                int last = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : first;
                for (int i = first; i <= last; i++) { entries[new KnyttPoint(bank, i)] = entry; }
                continue;
            }
            match = Regex.Match(line, @"^(\d+) (.+)$");
            if (match.Success) { bank_names[int.Parse(match.Groups[1].Value)] = match.Groups[2].Value.Trim(); }
        }
    }

    public static bool isCustom(int bank) => bank == 254 || bank == 255;

    public static bool isPlus(KnyttPoint id) => id.y != 0 && PLUS_OBJECTS.TryGetValue(id.x, out int first) && id.y >= first;

    public static KeyDataCollection customSection(KnyttWorld world, KnyttPoint id)
    {
        string name = $"Custom Object {(id.x == 254 ? "B" : "")}{id.y}";
        return world.INIData.Sections.ContainsSection(name) ? world.INIData[name] : null;
    }

    public IEnumerable<int> Banks => bank_names.Keys;

    public string bankName(int bank) => bank_names.TryGetValue(bank, out string name) ? name : "Empty";

    public IEnumerable<int> objects(int bank) =>
        isCustom(bank) ? Enumerable.Range(1, 255) : entries.Keys.Where(id => id.x == bank).Select(id => id.y).OrderBy(y => y);

    // Custom objects only exist once World.ini sets them up
    public bool exists(KnyttPoint id) => isCustom(id.x) ? customSection(world, id) != null : entries.ContainsKey(id);

    public string name(KnyttPoint id)
    {
        if (id.y == 0) { return "Erase"; }
        if (isCustom(id.x)) { return $"Custom object {(id.x == 254 ? "B" : "")}{id.y}"; }
        return entries.TryGetValue(id, out var entry) ? entry.name : "Nothing";
    }

    public string title(KnyttPoint id) => $"{id.x}:{id.y} {name(id)}";

    public string details(KnyttPoint id)
    {
        string text = id.y == 0 ? "Clears the cell on the current layer." :
                      isCustom(id.x) ? customDetails(id) :
                      entries.TryGetValue(id, out var entry) ? entry.description : "Not in the game.";
        return isPlus(id) ? (text == "" ? "" : text + "\n") + "Needs KS+." : text;
    }

    private string customDetails(KnyttPoint id)
    {
        var section = customSection(world, id);
        if (section == null) { return "Not set up in World.ini."; }

        var lines = new List<string>();
        int? bank = KnyttUtil.parseIniInt(section["Bank"]);
        int? obj = KnyttUtil.parseIniInt(section["Object"]);
        if (bank != null && obj != null) { lines.Add($"Works like {title(new KnyttPoint(bank.Value, obj.Value))}."); }
        if (section["Image"] != null)
        {
            string size = $"{section["Tile Width"] ?? "24"}x{section["Tile Height"] ?? "24"}";
            lines.Add($"Image {section["Image"]}, {size}.");
        }
        if (section["Hurts"]?.ToLower() == "false") { lines.Add("Safe to touch."); }
        return string.Join("\n", lines);
    }

    // Every word has to match the bank, number, name or description
    public IEnumerable<KnyttPoint> search(string text)
    {
        var words = text.ToLower().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (int bank in Banks)
        {
            foreach (int obj in objects(bank))
            {
                var id = new KnyttPoint(bank, obj);
                if (!exists(id)) { continue; }
                string haystack = $"{bankName(bank)} {title(id)} {details(id)}".ToLower();
                if (words.All(word => haystack.Contains(word))) { yield return id; }
            }
        }
    }
}
