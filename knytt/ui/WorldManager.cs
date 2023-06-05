using System.Collections.Generic;
using System.Linq;

public class WorldManager
{
    public List<WorldEntry> Filtered
    {
        get
        {
            var result = Entries.Where(checkEntry);
            switch (order)
            {
                case Order.Default:         break;
                case Order.ByLastPlayed:    result = result.OrderByDescending(e => e.LastPlayedTime); break;
                case Order.ByInstalledTime: result = result.OrderByDescending(e => e.InstalledTime); break;
                case Order.ByName:          result = result.OrderBy(e => e.Name); break;
                case Order.ByAuthor:        result = result.OrderBy(e => e.Author); break;
            }
            return result.ToList();
        }
    }

    private string category;
    private string difficulty;
    private string size;
    private string text;

    public enum Order { Default, ByLastPlayed, ByInstalledTime, ByName, ByAuthor }
    private Order order;

    List<WorldEntry> Entries { get; } = new List<WorldEntry>();

    public bool addWorld(WorldEntry entry)
    {
        this.Entries.Add(entry);
        return checkEntry(entry);
    }

    public void setFilter(string category, string difficulty, string size, string text, Order order)
    {
        this.category = category;
        this.difficulty = difficulty;
        this.size = size;
        this.text = text?.ToLower();
        this.order = order;
    }

    public bool checkEntry(WorldEntry entry)
    {
        if (category != null && !entry.Categories.Contains(category)) { return false; }
        if (difficulty != null && !entry.Difficulties.Contains(difficulty)) { return false; }
        if (size != null && !entry.Size.Equals(size)) { return false; }
        if (text != null && !entry.Name.ToLower().Contains(text) && !entry.Author.ToLower().Contains(text)) { return false; }
        return true;
    }

    public void clearAll()
    {
        this.Entries.Clear();
    }
}
