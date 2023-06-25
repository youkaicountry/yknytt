using Godot;
using IniParser.Model;
using IniParser.Parser;
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
        modified |= ensureSetting("Input", "map0", "Key(M)");
        modified |= ensureSetting("Input", "debug_die0", "Key(F2)");

        modified |= ensureSetting("Input", "up1", "Joy(12)");
        modified |= ensureSetting("Input", "down1", "Joy(13)");
        modified |= ensureSetting("Input", "left1", "Joy(14)");
        modified |= ensureSetting("Input", "right1", "Joy(15)");
        modified |= ensureSetting("Input", "show_info1", "Joy(3)");
        modified |= ensureSetting("Input", "jump1", "Joy(2)");
        modified |= ensureSetting("Input", "walk1", "Joy(6)");
        modified |= ensureSetting("Input", "umbrella1", "Joy(7)");
        modified |= ensureSetting("Input", "hologram1", "Joy(0)");
        modified |= ensureSetting("Input", "pause1", "Joy(4)");
        modified |= ensureSetting("Input", "map1", "Joy(5)");
        modified |= ensureSetting("Input", "debug_die1", "Joy(11)");

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

    private static string[] XBOX_BUTTONS = {
        "XBox A", "XBox B", "XBox X", "XBox Y", "L Bumper", "R Bumper", "L Trigger", "R Trigger", "L3", "R3", 
        "Select", "Start", "D-pad Up", "D-pad Down", "D-pad Left", "D-pad Right",
        "Home", "XBox Share", "Paddle 1", "Paddle 2", "Paddle 3", "Paddle 4", "Touchpad"
    };

    public static string getValueString(string ini_name)
    {
        if (!ini["Input"].ContainsKey(ini_name)) { return ""; }
        Match match = key_rx.Match(ini["Input"][ini_name]);
        var groups = match.Groups;

        switch (groups["type"].Value)
        {
            case "Key": return groups["value"].Value;
            case "Joy": return int.TryParse(groups["value"].Value, out var i) && i < XBOX_BUTTONS.Length ? 
                            /*Input.GetJoyButtonString(i)*/ XBOX_BUTTONS[i] : $"Joy {groups["value"]}";
        }

        return "";
    }

    public static bool setAction(string ini_name, InputEvent ev)
    {
        switch (ev)
        {
            case InputEventKey key:
                var keyname = OS.GetScancodeString(key.Scancode);
                ini["Input"][ini_name] = $"Key({keyname})";
                break;

            case InputEventJoypadButton jb:
                ini["Input"][ini_name] = $"Joy({jb.ButtonIndex})";
                break;

            case null:
                ini["Input"].RemoveKey(ini_name);
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
        applyAction("map");
        applyAction("debug_die");
        applyKey("debug_die", "r", ctrl: true);
    }

    private static void applyAction(string name)
    {
        // Clear the action
        InputMap.ActionEraseEvents(name);
        applyInput(name, name + "0");
        applyInput(name, name + "1");
    }

    private static void applyInput(string action_name, string ini_name)
    {
        if (!ini["Input"].ContainsKey(ini_name)) { return; }
        Match match = key_rx.Match(ini["Input"][ini_name]);
        var groups = match.Groups;

        switch (groups["type"].Value)
        {
            case "Key": applyKey(action_name, groups["value"].Value); break;
            case "Joy": applyJoy(action_name, groups["value"].Value); break;
        }
    }

    private static void applyKey(string action_name, string key, bool ctrl = false)
    {
        int scancode = OS.FindScancodeFromString(key);
        var e = new InputEventKey();
        e.Scancode = (uint)scancode;
        e.Control = ctrl;
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
