using Godot;

// A typed letter for its form's keys: a custom object's number, a cutscene's name
public class IniLetterEdit : LineEdit
{
    [Export] public int Slot { get; set; }
}
