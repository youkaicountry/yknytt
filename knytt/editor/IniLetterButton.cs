using Godot;

// Chooses a letter for its form's keys: Sign B, Custom Object B5
public class IniLetterButton : Button
{
    [Export] public string Letter { get; set; } = "";
    [Export] public int Slot { get; set; }
}
