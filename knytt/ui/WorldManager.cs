using System.Collections.Generic;

public class WorldManager
{
    public List<WorldEntry> Filtered 
    { 
        get
        {
            var result = new List<WorldEntry>();

            foreach (var entry in Entries)
            {
                if (!checkEntry(entry)) { continue; }
                result.Add(entry);
            }

            return result;
        }
    }

    private string category;
    private string difficulty;
    private string size;

    List<WorldEntry> Entries { get; } = new List<WorldEntry>();

    public bool addWorld(WorldEntry entry)
    {
        this.Entries.Add(entry);
        return checkEntry(entry);
    }

    public void setFilter(string category, string difficulty, string size)
    {
        this.category = category;
        this.difficulty = difficulty;
        this.size = size;
    }

    public bool checkEntry(WorldEntry entry)
    {
        if (category != null && !entry.Categories.Contains(category)) { return false; }
        if (difficulty != null && !entry.Difficulties.Contains(difficulty)) { return false; }
        if (size != null && !entry.Size.Equals(size)) { return false; }
        return true;
    }
}
