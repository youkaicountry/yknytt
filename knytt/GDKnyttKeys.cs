using Godot;
using IniParser.Model;
using IniParser.Parser;
using System.Text.RegularExpressions;

public class GDKnyttKeys : Node
{
    static IniData ini;
    static Regex key_rx;

    static GDKnyttKeys()
    {
        ini = new IniData();
        key_rx = new Regex(@"(?<type>\w+)\((?<value>[\w+\d]+)\)", RegexOptions.Compiled);
    }

    public override void _Ready()
    {
        initialize();
    }

    private void initialize()
    {
        bool modified = false;

        // Try to load the settings file
        modified |= loadSettings();

        modified |= ensureSetting("Input", "up0", "Key(Up)");
        modified |= ensureSetting("Input", "down0", "Key(Down)");
        modified |= ensureSetting("Input", "left0", "Key(Left)");
        modified |= ensureSetting("Input", "right0", "Key(Right)");
        modified |= ensureSetting("Input", "show_info0", "Key(Q)");
        modified |= ensureSetting("Input", "jump0", "Key(S)");
        modified |= ensureSetting("Input", "walk0", "Key(A)");
        modified |= ensureSetting("Input", "umbrella0", "Key(D)");
        modified |= ensureSetting("Input", "hologram0", "Key(W)");

        if (modified) { saveSettings(); }
        applyAllSettings();
    }

    // returns true if error
    public static bool loadSettings()
    {
        var f = new File();
        var error = f.Open("user://input.ini", File.ModeFlags.Read);
        if (error != Error.Ok) { return true; }
        var ini_text = f.GetAsText();
        f.Close();
        var parser = new IniDataParser();
        ini = parser.Parse(ini_text); // TODO: Handle malformed text (catch Exception, return true)
        return false;
    }

    public static void saveSettings()
    {
        var text = ini.ToString();
        var f = new File();
        f.Open("user://input.ini", File.ModeFlags.Write);
        f.StoreString(text);
        f.Close();
    }

    private static void applyAllSettings()
    {
        applyAction("up");
        applyAction("down");
        applyAction("left");
        applyAction("right");
        applyAction("show_info");
        applyAction("jump");
        applyAction("walk");
        applyAction("umbrella");
        applyAction("hologram");
    }

    private static void applyAction(string name)
    {
        // Clear the action
        InputMap.ActionEraseEvents(name);
        applyInput(name, name+"0");
        applyInput(name, name+"1");
    }

    private static void applyInput(string action_name, string ini_name)
    {
        if (!ini["Input"].ContainsKey(ini_name)) { return; }
        Match match = key_rx.Match(ini["Input"][ini_name]);
        var groups = match.Groups;
        //GD.Print("HERE");
        switch(groups["type"].Value)
        {
            case "Key": applyKey(action_name, groups["value"].Value); break;
        }
        //GD.Print("Type: ", groups["type"].Name);
        //GD.Print(groups["value"]);
    }

    private static void applyKey(string action_name, string key)
    {
        int scancode = OS.FindScancodeFromString(key);
        var e = new InputEventKey();
        e.Scancode = (uint)scancode;
        InputMap.ActionAddEvent(action_name, e);
    }

    public static bool ensureSetting(string section, string setting, string value)
    {
        bool modified = false;
        if (!ini.Sections.ContainsSection(section))
        { 
            ini.Sections.AddSection(section); 
            modified = true;
        }

        if (!ini[section].ContainsKey(setting))
        {
            ini[section][setting] = value;
            modified = true;
        }

        return modified;
    }
}
