using Godot;
using IniParser.Model;
using IniParser.Parser;

public class GDKnyttSettings : Node
{
    //OS.set_window_maximized(true), and then OS.set_borderless_window(true)
    public static IniData ini { get; private set; }
    static SceneTree tree;

    public enum ScrollTypes
    {
        Smooth = 0,
        Original = 1
    }

    static GDKnyttSettings()
    {
        ini = new IniData();
    }

    public static bool Fullscreen
    {
        get { return ini["Graphics"]["Fullscreen"].Equals("1") ? true : false; }
        set 
        {
            OS.WindowBorderless = value;
            OS.WindowMaximized = value;
            ini["Graphics"]["Fullscreen"] = value ? "1" : "0";
        }
    }

    public static bool SmoothScaling
    {
        get { return ini["Graphics"]["Smooth Scaling"].Equals("1") ? true : false; }
        set 
        {
			if (!TouchSettings.EnablePanel)
			{
				tree.SetScreenStretch(value ? SceneTree.StretchMode.Mode2d : SceneTree.StretchMode.Viewport, 
									  SceneTree.StretchAspect.Keep, new Vector2(600, 240));
			}
			else
			{
				// Touch panel needs some screen space, so "Viewport" mode is not suitable here
				tree.SetScreenStretch(SceneTree.StretchMode.Mode2d, 
									  SceneTree.StretchAspect.KeepWidth, new Vector2(600, 240));
			}
			(tree.Root.FindNode("GKnyttGame", owned: false) as GDKnyttGame)?.setupCamera();

            ini["Graphics"]["Smooth Scaling"] = value ? "1" : "0";
        }
    }

    public static ScrollTypes ScrollType
    {
        get { return String2ScrollTypes(ini["Graphics"]["Scroll Type"]); }
        set { ini["Graphics"]["Scroll Type"] = ScrollTypes2String(value); }
    }

    private static ScrollTypes String2ScrollTypes(string s)
    {
        switch(s)
        {
            case "Smooth": return ScrollTypes.Smooth;
            case "Original": return ScrollTypes.Original;
            default: return ScrollTypes.Smooth;
        }
    }

    private static string ScrollTypes2String(ScrollTypes t)
    {
        switch(t)
        {
            case ScrollTypes.Smooth: return "Smooth";
            case ScrollTypes.Original: return "Original";
            default: return "Smooth";
        }
    }

    public override void _Ready()
    {
        tree = GetTree();
        initialize();
    }

    private static void initialize()
    {
        bool modified = false;

        // Try to load the settings file
        modified |= loadSettings();

        modified |= ensureSetting("Graphics", "Fullscreen", "0");
        modified |= ensureSetting("Graphics", "Smooth Scaling", "0");
        modified |= ensureSetting("Graphics", "Scroll Type", "Smooth");
  
        modified |= TouchSettings.ensureSettings();

        if (modified) { saveSettings(); }
        applyAllSettings();
    }

    private static void applyAllSettings()
    {
        Fullscreen = ini["Graphics"]["Fullscreen"].Equals("1") ? true : false;
        SmoothScaling = ini["Graphics"]["Smooth Scaling"].Equals("1") ? true : false;
        ScrollType = String2ScrollTypes(ini["Graphics"]["Scroll Type"]);
    }

    public static void saveSettings()
    {
        var text = ini.ToString();
        var f = new File();
        f.Open("user://settings.ini", File.ModeFlags.Write);
        f.StoreString(text);
        f.Close();
    }

    // returns true if error
    public static bool loadSettings()
    {
        var f = new File();
        var error = f.Open("user://settings.ini", File.ModeFlags.Read);
        if (error != Error.Ok) { return true; }
        var ini_text = f.GetAsText();
        f.Close();
        var parser = new IniDataParser();
        ini = parser.Parse(ini_text); // TODO: Handle malformed text (catch Exception, return true)
        return false;
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

    private static bool setSetting(string section, string setting, string value)
    {
        string oval = ini[section][setting];
        if (oval.Equals(value)) { return false; }
        ini[section][setting] = value;
        return true;
    }
}
