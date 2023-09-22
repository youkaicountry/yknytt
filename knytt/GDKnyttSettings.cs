using Godot;
using System;
using IniParser.Model;
using IniParser.Parser;
using YKnyttLib;

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

    public static bool Mobile => OS.GetName() == "Android" || OS.GetName() == "iOS";

    public static bool Fullscreen
    {
        get { return ini["Graphics"]["Fullscreen"].Equals("1") ? true : false; }
        set
        {
            OS.WindowBorderless = value;
            OS.WindowMaximized = value;
            if (!value && OS.GetName() == "Windows")
            {
                OS.WindowSize = new Vector2((int)ProjectSettings.GetSetting("display/window/size/test_width"), 
                                            (int)ProjectSettings.GetSetting("display/window/size/test_height"));
                OS.WindowPosition = (OS.GetScreenSize() - OS.WindowSize) / 2;
            }
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
            tree.Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame")?.setupCamera();

            var font = ResourceLoader.Load<DynamicFont>("res://knytt/ui/UIDynamicFont.tres");
            font.FontData = ResourceLoader.Load<DynamicFontData>(
                    "res://knytt/fonts/" + (value ? "segan/Segan-Light.ttf" : "silver/Silver-Light.ttf"));
            font.Size = value ? 12 : 17;
            font.ExtraSpacingTop = value ? 1 : 0;
            font.ExtraSpacingBottom = value ? 0 : -6;
            font.UseFilter = value;
        }
    }

    public static void setupViewport(bool for_ui = false)
    {
        tree.SetScreenStretch(
            SmoothScaling ? SceneTree.StretchMode.Mode2d : SceneTree.StretchMode.Viewport, 
            for_ui || TouchSettings.EnablePanel ? SceneTree.StretchAspect.KeepWidth : SceneTree.StretchAspect.Keep,
            new Vector2(for_ui ? 600 : TouchSettings.ScreenWidth, 240));
    }
    
    public static ScrollTypes ScrollType
    {
        get { return String2ScrollTypes(ini["Graphics"]["Scroll Type"]); }
        set
        {
            var game = tree.Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame");
            if (game != null)
            {
                game.GDWorld.Areas.BorderSize = (value == ScrollTypes.Original ? new KnyttPoint(0, 0) : new KnyttPoint(1, 1));
            }
            ini["Graphics"]["Scroll Type"] = ScrollTypes2String(value);
        }
    }

    public static bool Border
    {
        get { return ini["Graphics"]["Border"].Equals("1") ? true : false; }
        set
        {
            ini["Graphics"]["Border"] = value ? "1" : "0";
            tree.Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame")?.setupBorder();
        }
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

    public static float EffectsPanning
    {
        get { return float.Parse(ini["Audio"]["Effects Panning"]); }
        set
        {
            ini["Audio"]["Effects Panning"] = $"{value}";
            // AudioStreamPlayer2D caches global project setting and never updates it
            // As a workaround, we need to set PanningStrength before Play() for every Player
            SFXAudioPlayer2D.GlobalPanningStrength = value;
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
            if (!ini.Sections.ContainsSection("Server") || !ini["Server"].ContainsKey("UUID"))
            {
                ensureSetting("Server", "UUID", System.Guid.NewGuid().ToString());
                saveSettings();
            }

            return ini["Server"]["UUID"];
        }
    }

    public static string ServerURL => ini["Server"]["URL"];

    public enum ConnectionType { Online, NoAchievements, Offline };

    public static ConnectionType Connection
    {
        get { return (ConnectionType)Enum.Parse(typeof(ConnectionType), ini["Server"]["Connection"]); }
        set { ini["Server"]["Connection"] = value.ToString(); }
    }

    public static string WorldsDirectory
    {
        get { return ini["Directories"]["Worlds"]; }
        set { ini["Directories"]["Worlds"] = value; }
    }

    public static bool WorldsDirForDownload
    {
        get { return ini["Directories"]["For Download"] == "1"; }
        set { ini["Directories"]["For Download"] = value ? "1" : "0"; }
    }

    public static string SavesDirectory
    {
        get { return ini["Directories"]["Saves"]; }
        set { ini["Directories"]["Saves"] = value; }
    }

    public static string Saves => SavesDirectory == "" ? "user://Saves" : SavesDirectory;

    public static float StickSensitivity
    {
        get { return float.Parse(ini["Misc"]["Stick Sensitivity"]); }
        set { ini["Misc"]["Stick Sensitivity"] = value.ToString(); GDKnyttKeys.StickTreshold = 1 - value; }
    }

    public override void _Ready()
    {
        tree = GetTree();
        initialize();
    }

    private static void moveUserDir()
    {
        if (OS.GetName() == "Windows" || OS.GetName() == "X11")
        {
            string prev_data_dir = OS.GetUserDataDir().PlusFile("../godot/app_userdata/YKnytt");
            if (System.IO.File.Exists(prev_data_dir.PlusFile("input.ini")) && !new Directory().FileExists("user://input.ini"))
            {
                System.IO.File.Move(prev_data_dir.PlusFile("input.ini"), OS.GetUserDataDir().PlusFile("input.ini"));
            }
            if (System.IO.File.Exists(prev_data_dir.PlusFile("settings.ini")) && !new Directory().FileExists("user://settings.ini"))
            {
                System.IO.File.Move(prev_data_dir.PlusFile("settings.ini"), OS.GetUserDataDir().PlusFile("settings.ini"));
            }
            if (System.IO.Directory.Exists(prev_data_dir.PlusFile("Saves")) && !new Directory().DirExists("user://Saves"))
            {
                System.IO.Directory.Move(prev_data_dir.PlusFile("Saves"), OS.GetUserDataDir().PlusFile("Saves"));
            }
            if (System.IO.Directory.Exists(prev_data_dir.PlusFile("Worlds")) && !new Directory().DirExists("user://Worlds"))
            {
                System.IO.Directory.Move(prev_data_dir.PlusFile("Worlds"), OS.GetUserDataDir().PlusFile("Worlds"));
            }
        }
    }

    private static void initialize()
    {
        bool modified = false;

        moveUserDir();

        // Try to load the settings file
        modified |= loadSettings();

        modified |= checkVersion();

        modified |= ensureSetting("Graphics", "Fullscreen", "0");
        modified |= ensureSetting("Graphics", "Smooth Scaling", "1");
        modified |= ensureSetting("Graphics", "Scroll Type", "Original");
        modified |= ensureSetting("Graphics", "Border", Mobile && TouchSettings.isHandsOverlapping() ? "1" : "0");

        modified |= ensureSetting("Audio", "Master Volume", "100");
        modified |= ensureSetting("Audio", "Music Volume", "80");
        modified |= ensureSetting("Audio", "Environment Volume", "80");
        modified |= ensureSetting("Audio", "Effects Volume", "70");
        modified |= ensureSetting("Audio", "Effects Panning", "1");

        modified |= ensureSetting("Server", "URL", (OS.GetName() == "HTML5" ? "https" : "http") + "://yknytt.pythonanywhere.com");
        modified |= ensureSetting("Server", "Connection", "Online");

        modified |= ensureSetting("Directories", "Worlds", "");
        modified |= ensureSetting("Directories", "Saves", "");
        modified |= ensureSetting("Directories", "For Download", "0");

        modified |= ensureSetting("Misc", "Stick Sensitivity", (0.7f).ToString());

        modified |= TouchSettings.ensureSettings();

        if (modified) { saveSettings(); }
        applyAllSettings();
    }

    private static void applyAllSettings()
    {
        Fullscreen = ini["Graphics"]["Fullscreen"].Equals("1") ? true : false;
        SmoothScaling = ini["Graphics"]["Smooth Scaling"].Equals("1") ? true : false;
        ScrollType = String2ScrollTypes(ini["Graphics"]["Scroll Type"]);
        Border = ini["Graphics"]["Border"].Equals("1") ? true : false;
        MasterVolume = int.Parse(ini["Audio"]["Master Volume"]);
        MusicVolume = int.Parse(ini["Audio"]["Music Volume"]);
        EnvironmentVolume = int.Parse(ini["Audio"]["Environment Volume"]);
        EffectsVolume = int.Parse(ini["Audio"]["Effects Volume"]);
        EffectsPanning = float.Parse(ini["Audio"]["Effects Panning"]);
        StickSensitivity = float.Parse(ini["Misc"]["Stick Sensitivity"]);
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

    private static bool checkVersion()
    {
        if (!ini.Sections.ContainsSection("Misc")) { ini.Sections.AddSection("Misc"); }

        if (!ini["Misc"].ContainsKey("Version") || ini["Misc"]["Version"] != "0.6")
        {
            ini["Misc"]["Version"] = "0.6";
            if (OS.GetName() == "HTML5") { ini["Server"]?.RemoveKey("URL"); }
            if (new File().FileExists("user://input.ini"))
            {
                GDKnyttKeys.loadSettings();
                GDKnyttKeys.setAction("debug_die0", new InputEventKey() {Scancode = (uint)KeyList.F10});
                GDKnyttKeys.saveSettings();
            }
            GDKnyttWorldImpl.removeDirectory("user://Cache");
            return true;
        }
        return false;
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("fullscreen")) { Fullscreen = !Fullscreen; }
        if (Input.IsActionJustPressed("border")) { Border = !Border; }

        if (Input.IsActionJustPressed("zoom") && ! Fullscreen)
        {
            Vector2 max_size = OS.GetScreenSize(), cur_size = OS.WindowSize,
                min_size = new Vector2((int)ProjectSettings.GetSetting("display/window/size/width"), 
                                       (int)ProjectSettings.GetSetting("display/window/size/height"));
            int scale = (int)Mathf.Min(cur_size.x / min_size.x, cur_size.y / min_size.y);
            int max_scale = (int)Mathf.Min(max_size.x / min_size.x, max_size.y / min_size.y);
            
            scale++;
            if (scale > max_scale) { scale = 1; }
            OS.WindowSize = min_size * scale;
            OS.WindowPosition = (max_size - OS.WindowSize) / 2;
            OS.WindowMaximized = false;
        }
    }
}
