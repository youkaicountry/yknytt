using Godot;

public static class WorldName
{
    private const string INVALID_CHARACTERS = "\\/:*?\"<>|";

    public static string path(string name, string author) => LevelSelection.InstallDirectory.PlusFile($"{author} - {name}.knytt.bin");

    public static string check(string name, string author)
    {
        if (name == "" || author == "") { return "Enter a level name and author"; }
        if ((name + author).IndexOfAny(INVALID_CHARACTERS.ToCharArray()) != -1) { return $"Names can't contain {INVALID_CHARACTERS}"; }
        if (new File().FileExists(path(name, author))) { return "This level already exists"; }
        return null;
    }
}
