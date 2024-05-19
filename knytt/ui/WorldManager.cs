using System;
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
                case Order.ByFileSize:      result = result.OrderByDescending(e => e.FileSize); break;
                case Order.ByCompletion:    result = result.OrderBy(
                    e => e.Completed <= 0 && e.HasSaves ? 1 : completed_dict[e.Completed]); break;
            }
            return result.ToList();
        }
    }

    Dictionary<int, int> completed_dict = new Dictionary<int, int> { [2] = 0, [-1] = 2, [0] = 2, [3] = 3, [5] = 4, [4] = 5, [6] = 6, [1] = 7 };

    private string category;
    private string difficulty;
    private string size;
    private string text;

    public enum Order { Default, ByLastPlayed, ByInstalledTime, ByName, ByAuthor, ByFileSize, ByCompletion }
    private Order order;

    List<string> size_choices = new List<string>(){ "small", "medium", "large" };
    List<string> difficulty_choices = new List<string>(){ "easy", "normal", "hard", "very Hard", "lunatic"};
    List<string> category_choices = new List<string>(){ "tutorial", "challenge", "puzzle", "maze", "environmental", "playground", "misc"};

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
        var cmp = StringComparer.OrdinalIgnoreCase;
        if (category != null && !entry.Categories.Contains(category, cmp)) { return false; }
        if (category == "" && entry.Categories.Intersect(category_choices, cmp).Count() > 0) { return false; }
        if (difficulty != null && !entry.Difficulties.Contains(difficulty, cmp)) { return false; }
        if (difficulty == "" && entry.Difficulties.Intersect(difficulty_choices, cmp).Count() > 0) { return false; }
        if (size != null && cmp.Compare(entry.Size, size) != 0) { return false; }
        if (size == "" && size_choices.Contains(entry.Size, cmp)) { return false; }
        if (text != null && 
            !entry.Name.ToLower().Contains(text) && 
            !entry.Author.ToLower().Contains(text) &&
            !(entry.Description != null && entry.Description.ToLower().Contains(text))) { return false; }
        return true;
    }

    public void removeWorld(WorldEntry entry)
    {
        this.Entries.Remove(entry);
    }

    public void clearAll()
    {
        this.Entries.Clear();
    }
}
