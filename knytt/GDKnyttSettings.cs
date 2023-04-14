using Godot;
using IniParser.Model;
using IniParser.Parser;

public partial class GDKnyttSettings : Node
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
            DisplayServer.WindowSetMode(value ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
            //OS.WindowBorderless = value;
            //OS.WindowMaximized = value;
            ini["Graphics"]["Fullscreen"] = value ? "1" : "0";
        }
    }

    public static bool SmoothScaling
    {
        get { return ini["Graphics"]["Smooth Scaling"].Equals("1") ? true : false; }
        set
        {
            ini["Graphics"]["Smooth Scaling"] = value ? "1" : "0";
            setupViewport(for_ui: true);
            (tree.Root.FindChild("GKnyttGame", owned: false) as GDKnyttGame)?.setupCamera();
        }
    }
    
    public static void setupViewport(bool for_ui = false)
    {
        if (!TouchSettings.EnablePanel)
        {
            tree.Root.ContentScaleMode = SmoothScaling ? Window.ContentScaleModeEnum.CanvasItems : Window.ContentScaleModeEnum.Viewport;
            tree.Root.ContentScaleAspect = Window.ContentScaleAspectEnum.Keep;
            tree.Root.ContentScaleSize = new Vector2I(600, 240);
        }
        else
        {
            // Touch panel needs some screen space, so "Viewport" mode is not suitable here
            tree.Root.ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
            tree.Root.ContentScaleAspect = Window.ContentScaleAspectEnum.KeepWidth;
            tree.Root.ContentScaleSize = new Vector2I(for_ui ? 600 : TouchSettings.ScreenWidth, 240);
        }
    }
    
    public static ScrollTypes ScrollType
    {
        get { return String2ScrollTypes(ini["Graphics"]["Scroll Type"]); }
        set { ini["Graphics"]["Scroll Type"] = ScrollTypes2String(value); }
    }

    // Calculate the volume in dB from the config value
    private static float calcVolume(int v)
    {
        if (v == 0) { return -80f; }
        return (((float)v) / 100f) * 26f - 20f;
    }

    public static int MasterVolume
    {
        get { return int.Parse(ini["Audio"]["Master Volume"]); }
        set
        {
            ini["Audio"]["Master Volume"] = $"{value}";
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), calcVolume(value));
        }
    }

    public static int MusicVolume
    {
        get { return int.Parse(ini["Audio"]["Music Volume"]); }
        set
        {
            ini["Audio"]["Music Volume"] = $"{value}";
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("MusicVol"), calcVolume(value));
        }
    }

    public static int EffectsVolume
    {
        get { return int.Parse(ini["Audio"]["Effects Volume"]); }
        set
        {
            ini["Audio"]["Effects Volume"] = $"{value}";
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), calcVolume(value));
        }
    }

    public static int EnvironmentVolume
    {
        get { return int.Parse(ini["Audio"]["Environment Volume"]); }
        set
        {
            ini["Audio"]["Environment Volume"] = $"{value}";
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Environment"), calcVolume(value));
        }
    }

    private static ScrollTypes String2ScrollTypes(string s)
    {
        switch (s)
        {
            case "Smooth": return ScrollTypes.Smooth;
            case "Original": return ScrollTypes.Original;
            default: return ScrollTypes.Smooth;
        }
    }

    private static string ScrollTypes2String(ScrollTypes t)
    {
        switch (t)
        {
            case ScrollTypes.Smooth: return "Smooth";
            case ScrollTypes.Original: return "Original";
            default: return "Smooth";
        }
    }

    public static string UUID
    {
        get
        {
            if (!GDKnyttSettings.ini.Sections.ContainsSection("Server") || !GDKnyttSettings.ini["Server"].ContainsKey("UUID"))
            {
                GDKnyttSettings.ensureSetting("Server", "UUID", System.Guid.NewGuid().ToString());
                GDKnyttSettings.saveSettings();
            }

            return GDKnyttSettings.ini["Server"]["UUID"];
        }
    }

    public static string ServerURL
    {
        get { return GDKnyttSettings.ini["Server"]["URL"]; }
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

        modified |= ensureSetting("Audio", "Master Volume", "100");
        modified |= ensureSetting("Audio", "Music Volume", "80");
        modified |= ensureSetting("Audio", "Effects Volume", "70");
        modified |= ensureSetting("Audio", "Environment Volume", "80");

        modified |= ensureSetting("Server", "URL", "http://yknytt.fly.dev");

        modified |= TouchSettings.ensureSettings();

        if (modified) { saveSettings(); }
        applyAllSettings();
    }

    private static void applyAllSettings()
    {
        Fullscreen = ini["Graphics"]["Fullscreen"].Equals("1") ? true : false;
        SmoothScaling = ini["Graphics"]["Smooth Scaling"].Equals("1") ? true : false;
        ScrollType = String2ScrollTypes(ini["Graphics"]["Scroll Type"]);
        MasterVolume = int.Parse(ini["Audio"]["Master Volume"]);
        MusicVolume = int.Parse(ini["Audio"]["Music Volume"]);
        EffectsVolume = int.Parse(ini["Audio"]["Effects Volume"]);
        EnvironmentVolume = int.Parse(ini["Audio"]["Environment Volume"]);
    }

    public static void saveSettings()
    {
        var text = ini.ToString();
        using var f = FileAccess.Open("user://settings.ini", FileAccess.ModeFlags.Write);
        f.StoreString(text);
    }

    // returns true if error
    public static bool loadSettings()
    {
        using var f = FileAccess.Open("user://settings.ini", FileAccess.ModeFlags.Read);
        if (f == null) { return true; }
        var ini_text = f.GetAsText();
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
