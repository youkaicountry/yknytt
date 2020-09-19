using Godot;
using IniParser.Model;
using IniParser.Parser;
using System.Diagnostics;
using System.Text.RegularExpressions;

// TODO: Make a general version of this
// It should have structs for each setting value that can produce / take events & strings

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
        modified |= ensureSetting("Input", "pause0", "Key(Escape)");

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

    public static string getValueString(string ini_name)
    {
        if (!ini["Input"].ContainsKey(ini_name)) { return ""; }
        Match match = key_rx.Match(ini["Input"][ini_name]);
        var groups = match.Groups;

        switch(groups["type"].Value)
        {
            case "Key": return groups["value"].Value;
            case "Joy": return string.Format("Joy {0}", groups["value"]);
        }

        return "";
    }

    public static bool setAction(string ini_name, InputEvent ev)
    {
        switch(ev)
        {
            case InputEventKey key:
                var keyname = OS.GetScancodeString(key.Scancode);
                ini["Input"][ini_name] = string.Format("Key({0})", keyname);
                break;
            
            case InputEventJoypadButton jb: 
                ini["Input"][ini_name] = string.Format("Joy({0})", jb.ButtonIndex);
                break;

            default: return false;
        }

        return true;
    }

    public static void applyAllSettings()
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
        applyAction("pause");
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

        switch(groups["type"].Value)
        {
            case "Key": applyKey(action_name, groups["value"].Value); break;
            case "Joy": applyJoy(action_name, groups["value"].Value); break;
        }
    }

    private static void applyKey(string action_name, string key)
    {
        int scancode = OS.FindScancodeFromString(key);
        var e = new InputEventKey();
        e.Scancode = (uint)scancode;
        InputMap.ActionAddEvent(action_name, e);
    }

    private static void applyJoy(string action_name, string key)
    {
        var e = new InputEventJoypadButton();
        //e.Device
        e.ButtonIndex = int.Parse(key);
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
