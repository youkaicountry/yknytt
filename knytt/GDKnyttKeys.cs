using Godot;
using IniParser.Model;
using IniParser.Parser;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// TODO: Make a general version of this
// It should have structs for each setting value that can produce / take events & strings

public class GDKnyttKeys : Node
{
    public static IniData ini;
    static Regex key_rx;
    static Dictionary<int, string> axis_map = new Dictionary<int, string>();

    static GDKnyttKeys()
    {
        ini = new IniData();
        key_rx = new Regex(@"(?<type>\w+)\((?<value>[\-\w+\d]+)\)", RegexOptions.Compiled);
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
        modified |= ensureSetting("Input", "debug_die0", "Key(F10)");

        modified |= ensureSetting("Input", "up1", "Joy(12)");
        modified |= ensureSetting("Input", "down1", "Joy(13)");
        modified |= ensureSetting("Input", "left1", "Joy(14)");
        modified |= ensureSetting("Input", "right1", "Joy(15)");
        modified |= ensureSetting("Input", "show_info1", "Joy(3)");
        modified |= ensureSetting("Input", "jump1", "Joy(2)");
        modified |= ensureSetting("Input", "walk1", OS.GetName() != "Unix" ? "Joy(6)" : "Joy(5)");
        modified |= ensureSetting("Input", "umbrella1", "Joy(7)");
        modified |= ensureSetting("Input", "hologram1", "Joy(1)");
        modified |= ensureSetting("Input", "pause1", OS.GetName() != "Unix" ? "Joy(4)" : "Joy(11)");
        modified |= ensureSetting("Input", "map1", OS.GetName() != "Unix" ? "Joy(5)" : "Joy(6)");
        modified |= ensureSetting("Input", "debug_die1", "Joy(10)");

        if (modified) { saveSettings(); }
        applyAllSettings();
    }

    // returns true if error
    public static bool loadSettings()
    {
        var f = new File();
        var error = f.Open(GDKnyttDataStore.BaseDataDirectory.PlusFile("input.ini"), File.ModeFlags.Read);
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
        f.Open(GDKnyttDataStore.BaseDataDirectory.PlusFile("input.ini"), File.ModeFlags.Write);
        f.StoreString(text);
        f.Close();
    }

    private static string[] XBOX_BUTTONS = {
        "XBox A", "XBox B", "XBox X", "XBox Y", "L Bumper", "R Bumper", "L Trigger", "R Trigger", "L Stick Push", "R Stick Push", 
        "Select", "Start", "D-pad Up", "D-pad Down", "D-pad Left", "D-pad Right",
        "Home", "XBox Share", "Paddle 1", "Paddle 2", "Paddle 3", "Paddle 4", "Touchpad"
    };

    private static string[] NINTENDO_BUTTONS = {"Nintendo B", "Nintendo A", "Nintendo Y", "Nintendo X"};

    private static Dictionary<int, string> AXIS_NAMES = new Dictionary<int, string> {
        [1] = "L Stick Right", [-1] = "L Stick Left", [2] = "L Stick Down", [-2] = "L Stick Up",
        [3] = "R Stick Right", [-3] = "R Stick Left", [4] = "R Stick Down", [-4] = "R Stick Up",
    };

    private static Dictionary<string, string> GPTOKEYB_MAPPING = new Dictionary<string, string> {
        ["E"] = "Nintendo B", ["W"] = "Nintendo A", ["S"] = "Nintendo Y", ["Q"] = "Nintendo X",
        ["Z"] = "L Bumper", ["M"] = "L Trigger", ["D"] = "R Trigger", ["A"] = "R Bumper",
        ["F10"] = "Select", ["Escape"] = "Start", ["X"] = "Home", ["C"] = "Max", ["V"] = "L3", ["B"] = "R3",
        ["Up"] = "D-pad Up", ["Down"] = "D-pad Down", ["Left"] = "D-pad Left", ["Right"] = "D-pad Right",
        ["R"] = "L Stick Push", ["U"] = "R Stick Push",
        ["Y"] = "L Stick Right", ["F"] = "L Stick Left", ["G"] = "L Stick Down", ["T"] = "L Stick Up",
        ["L"] = "R Stick Right", ["J"] = "R Stick Left", ["K"] = "R Stick Down", ["I"] = "R Stick Up",
    };

    public static string getValueString(string ini_name)
    {
        if (!ini["Input"].ContainsKey(ini_name)) { return ""; }
        Match match = key_rx.Match(ini["Input"][ini_name]);
        var groups = match.Groups;
        var value = groups["value"].Value;

        switch (groups["type"].Value)
        {
            case "Key":
                return GDKnyttDataStore.GptokeybMode && GPTOKEYB_MAPPING.ContainsKey(value) ? 
                    GPTOKEYB_MAPPING[value] : value;
            case "Joy":
                if (GDKnyttDataStore.GptokeybMode) { return ""; }
                if (!int.TryParse(value, out var i)) { return $"Joy {value}"; }
                if (i < NINTENDO_BUTTONS.Length && OS.GetName() == "Unix") { return NINTENDO_BUTTONS[i]; }
                if (i < XBOX_BUTTONS.Length) { return XBOX_BUTTONS[i]; }
                return $"Joy {value}";
            case "Axis":
                if (GDKnyttDataStore.GptokeybMode) { return ""; }
                return int.TryParse(value, out var j) && AXIS_NAMES.ContainsKey(j) ? 
                            AXIS_NAMES[j] : $"Axis {value}";
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

            case InputEventJoypadMotion jm:
                if (jm.Axis == 6 || jm.Axis == 7) { return false; }
                if (Mathf.Abs(jm.AxisValue) <= StickTreshold) { return false; }
                int value = jm.AxisValue > 0 ? jm.Axis + 1 : -jm.Axis - 1;
                ini["Input"][ini_name] = $"Axis({value})";
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
        axis_map = new Dictionary<int, string>();
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
        applyKey("down", "Enter");
        if (GDKnyttSettings.LeftStickMovement)
        {
            if (!GDKnyttDataStore.GptokeybMode)
            {
                applyAxis("up", "-2");
                applyAxis("down", "2");
                applyAxis("left", "-1");
                applyAxis("right", "1");
            }
            else
            {
                applyKey("up", "t");
                applyKey("down", "g");
                applyKey("left", "f");
                applyKey("right", "y");
            }
        }
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
            case "Axis": applyAxis(action_name, groups["value"].Value); break;
        }
    }

    private static void applyKey(string action_name, string key, bool ctrl = false)
    {
        int scancode = OS.FindScancodeFromString(key);
        var e = new InputEventKey() { Scancode = (uint)scancode, Control = ctrl };
        InputMap.ActionAddEvent(action_name, e);
    }

    private static void applyJoy(string action_name, string key)
    {
        var e = new InputEventJoypadButton() { ButtonIndex = int.Parse(key), Device = -1 };
        InputMap.ActionAddEvent(action_name, e);
    }

    private static void applyAxis(string action_name, string key)
    {
        if (axis_map.ContainsKey(int.Parse(key))) { return; }
        axis_map.Add(int.Parse(key), action_name);
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

    public static float StickTreshold = 0.3f;

    private static HashSet<int> pressed_axis = new HashSet<int>();

    public override void _Input(InputEvent @event)
    {
        if (!(@event is InputEventJoypadMotion jm)) { return; }

        int axis = jm.AxisValue > 0 ? jm.Axis + 1 : -jm.Axis - 1;
        
        if (axis_map.ContainsKey(axis))
        {
            if (Mathf.Abs(jm.AxisValue) > StickTreshold)
            {
                Input.ActionPress(axis_map[axis]);
                pressed_axis.Add(axis);
            }
            else
            {
                if (pressed_axis.Contains(axis))
                {
                    Input.ActionRelease(axis_map[axis]);
                    pressed_axis.Remove(axis);
                }
            }
        }

        if (axis_map.ContainsKey(-axis) && pressed_axis.Contains(-axis))
        {
            Input.ActionRelease(axis_map[-axis]);
            pressed_axis.Remove(-axis);
        }
    }
}
