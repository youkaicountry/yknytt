using Godot;
using IniParser.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// A control that edits one INI key. "" is not set
public interface IIniField
{
    string Section { get; }
    string Key { get; }
    string Value { get; set; }
}

// Fills its fields from INI sections and says what gets edited, as it's typed and when it's done.
// Sections and keys can hold the form's letters, chosen by its IniLetterButtons or typed in its IniLetterEdits:
// Sign{1}({0}) is Sign2(B) while B and page 2 are chosen
public class IniForm : Control
{
    [Signal] public delegate void Edited(string section, string key, string value);
    [Signal] public delegate void Done();
    [Signal] public delegate void Focused(string section, string key);

    // Empty for the screen being edited. A field's own Section wins
    [Export] public string Section { get; set; } = "";
    [Export] public string[] Letters { get; set; } = { "A" };

    private readonly List<IIniField> fields = new List<IIniField>();
    private readonly List<IniLetterButton> letter_buttons = new List<IniLetterButton>();
    private readonly List<IniLetterEdit> letter_edits = new List<IniLetterEdit>();
    private IniData ini;
    private string screen;

    public override void _Ready()
    {
        findFields(this);
    }

    private void findFields(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            var bind = new Godot.Collections.Array { child };
            if (child is IIniField field)
            {
                fields.Add(field);
                child.Connect("Edited", this, nameof(onFieldEdited), bind);
                child.Connect("Done", this, nameof(onFieldDone));
                child.Connect("focus_entered", this, nameof(onFieldFocused), bind);
            }
            else if (child is IniLetterButton button)
            {
                letter_buttons.Add(button);
                button.Connect("pressed", this, nameof(onLetterPressed), bind);
            }
            else if (child is IniLetterEdit edit)
            {
                letter_edits.Add(edit);
                edit.Connect("text_changed", this, nameof(onLetterTyped), bind);
            }
            findFields(child);
        }
    }

    public void show(IniData ini, string screen)
    {
        this.ini = ini;
        this.screen = screen;
        foreach (var field in fields) { field.Value = ini.Sections.GetSectionData(sectionOf(field))?.Keys?[keyOf(field)] ?? ""; }
    }

    private string sectionOf(IIniField field)
    {
        string section = field.Section != "" ? field.Section : Section;
        return section == "" ? screen : string.Format(section, Letters);
    }

    private string keyOf(IIniField field) => string.Format(field.Key, Letters);

    // The field that edits this key of this section, and the letters it needs. Sign2(B) is Text with B and page 2.
    // No key: the first field of the section
    public Control find(string section, string key, out string[] letters)
    {
        foreach (var field in fields)
        {
            letters = (string[])Letters.Clone();
            string pattern = field.Section != "" ? field.Section : Section;
            bool here = pattern == "" ? Regex.IsMatch(section, @"^x-?\d+y-?\d+$", RegexOptions.IgnoreCase) : match(pattern, section, letters);
            if (here && (key == null || match(field.Key, key, letters))) { return (Control)field; }
        }
        letters = null;
        return null;
    }

    // Matches text to a pattern like Sign{1}({0}), filling in its letters: a button's, or anything typed
    private bool match(string pattern, string text, string[] letters)
    {
        string regex = Regex.Escape(pattern);
        for (int slot = 0; slot < Letters.Length; slot++)
        {
            var choices = letter_buttons.Where(b => b.Slot == slot).Select(b => Regex.Escape(b.Letter)).OrderByDescending(l => l.Length).ToList();
            regex = regex.Replace($@"\{{{slot}}}", $"(?<l{slot}>{(choices.Count > 0 ? string.Join("|", choices) : ".+?")})");
        }
        var found = Regex.Match(text, $"^{regex}$", RegexOptions.IgnoreCase);
        if (!found.Success) { return false; }
        for (int slot = 0; slot < Letters.Length; slot++)
        {
            var group = found.Groups[$"l{slot}"];
            if (!group.Success) { continue; }
            letters[slot] = letter_buttons.FirstOrDefault(b => b.Slot == slot && b.Letter.ToLower() == group.Value.ToLower())?.Letter ?? group.Value;
        }
        return true;
    }

    // What the letter buttons and edits do
    public void choose(string[] letters)
    {
        for (int slot = 0; slot < Letters.Length && slot < letters.Length; slot++) { Letters[slot] = letters[slot]; }
        foreach (var button in letter_buttons)
        {
            if (button.Letter == Letters[button.Slot]) { button.Pressed = true; }
        }
        foreach (var edit in letter_edits)
        {
            if (edit.Text.Trim() != Letters[edit.Slot]) { edit.Text = Letters[edit.Slot]; }
        }
        if (ini != null) { show(ini, screen); }
    }

    private void onLetterPressed(Node button)
    {
        typed(((IniLetterButton)button).Slot, ((IniLetterButton)button).Letter);
    }

    private void onLetterTyped(string text, Node edit)
    {
        typed(((IniLetterEdit)edit).Slot, text.Trim());
    }

    private void typed(int slot, string letter)
    {
        Letters[slot] = letter;
        if (ini != null) { show(ini, screen); }
    }

    private void onFieldEdited(Node field)
    {
        EmitSignal(nameof(Edited), sectionOf((IIniField)field), keyOf((IIniField)field), ((IIniField)field).Value);
    }

    private void onFieldDone()
    {
        EmitSignal(nameof(Done));
    }

    private void onFieldFocused(Node field)
    {
        EmitSignal(nameof(Focused), sectionOf((IIniField)field), keyOf((IIniField)field));
    }
}
