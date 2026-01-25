using Godot;
using System;
using IniParser.Model;
using IniParser.Parser;
using YKnyttLib;
using System.Linq;
using System.Globalization;

public partial class GDKnyttSettings : Node
{
    public static IniData ini { get; private set; }
    static SceneTree tree;

    public enum ShaderType { NoShader, HQ4X, CRT, Sepia, VHS, }

    static GDKnyttSettings()
    {
        ini = new IniData();
    }

    // Device with touchscreen and virtual keyboard
    public static bool Mobile => OS.GetName() == "Android" || OS.GetName() == "iOS";

    public static bool Fullscreen
    {
        get { return ini["Graphics"]["Fullscreen"].Equals("1") ? true : false; }
        set
        {
            // Godot 4: Use Set methods instead of trying to assign to Get methods
            DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, value);
            DisplayServer.WindowSetMode(value ? DisplayServer.WindowMode.Maximized : DisplayServer.WindowMode.Windowed);
            if (!value && OS.GetName() == "Windows")
            {
                var size = new Vector2I((int)(long)ProjectSettings.GetSetting("display/window/size/viewport_width"),
                                        (int)(long)ProjectSettings.GetSetting("display/window/size/viewport_height"));
                DisplayServer.WindowSetSize(size);
                DisplayServer.WindowSetPosition((DisplayServer.ScreenGetSize() - size) / 2);
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
            setupViewport(for_ui: true, check_shrink: true);
            tree.Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame")?.setupCamera();

            // TODO: Godot 4 font system changed - DynamicFont replaced with FontFile
            // Font customization needs to be reimplemented with the new font system
            // var font = ResourceLoader.Load<Font>("res://knytt/ui/UIDynamicFont.tres");
            // font.FontData, font.Size, font.ExtraSpacing*, font.UseFilter all need new approach
        }
    }

    public static void setupViewport(bool for_ui = false, bool check_shrink = false)
    {
        // Godot 4: Stretch settings are now on the root Window, not SceneTree
        float ui_width = check_shrink && DisplayServer.WindowGetSize().Aspect() <= 1.5f ? 530 : 600;
        var root = tree.Root;
        root.ContentScaleMode = SmoothScaling ? Window.ContentScaleModeEnum.CanvasItems : Window.ContentScaleModeEnum.Viewport;
        root.ContentScaleAspect = for_ui || TouchSettings.EnablePanel ? Window.ContentScaleAspectEnum.KeepWidth : Window.ContentScaleAspectEnum.Keep;
        root.ContentScaleSize = new Vector2I((int)(for_ui ? ui_width : TouchSettings.ScreenWidth), 240);
    }

    public static void setupCamera()
    {
        var game = tree.Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame");
        if (game != null)
        {
            game.GDWorld.Areas.BorderSize =
                SeamlessScroll ? new KnyttPoint(1, 0) : new KnyttPoint(0, 0);

            game.setupCamera();

            if (SideScroll)
            {
                game.adjustCenteredScroll(initial: true);
            }
            else
            {
                game.Camera.jumpTo(game.CurrentArea.GlobalCenter);
                foreach (var area in game.GDWorld.Areas.Areas.Values)
                {
                    area.Background.Position = Vector2.Zero;
                }
            }
        }
    }

    public static bool SideScroll => SeamlessScroll || Aspect > 0.4f;

    public static float Aspect
    {
        get { return float.Parse(ini["Graphics"]["Aspect"]); }
        set { ini["Graphics"]["Aspect"] = $"{value}"; }
    }

    public static bool SeamlessScroll
    {
        get { return ini["Graphics"]["Seamless"].Equals("1"); }
        set { ini["Graphics"]["Seamless"] = value ? "1" : "0"; }
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

    public static bool ForcedMap
    {
        get { return ini["Graphics"]["Forced Map"].Equals("1") ? true : false; }
        set
        {
            var game = tree.Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame");
            bool had_map = game != null && game.hasMap();
            ini["Graphics"]["Forced Map"] = value ? "1" : "0";
            if (game == null) { return; }
            if (!had_map && game.hasMap()) { game.UI.forceMap(true); }
            if (had_map && !game.hasMap()) { game.UI.forceMap(false); }
        }
    }

    public static bool DetailedMap
    {
        get { return ini["Graphics"]["Detailed Map"].Equals("1") ? true : false; }
        set
        {
            ini["Graphics"]["Detailed Map"] = value ? "1" : "0";
            var game = tree.Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame");
            var viewports = game?.GetNode<MapViewports>("%MapViewports");
            var map_panel = game?.GetNode<MapPanel>("UICanvasLayer/MapBackgroundPanel/MapPanel");
            MapPanel.initScale();
            if (game == null) { return; }
            if (value)
            {
                viewports.init(game.GDWorld.KWorld);
                if (game.hasMap()) { viewports.addArea(game.CurrentArea); }
            }
            else { viewports.destroy(); }
            map_panel.initSize();
        }
    }
    
    public static ShaderType Shader
    {
        get { return Enum.TryParse<ShaderType>(ini["Graphics"]["Shader Type"], out var s) ? s : ShaderType.NoShader; }
        set
        {
            ini["Graphics"]["Shader Type"] = value.ToString();
            tree.Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame")?.setupShader();
        }
    }

    public static bool WSOD
    {
        get { return ini["Graphics"]["WSOD"].Equals("1") ? true : false; }
        set { ini["Graphics"]["WSOD"] = value ? "1" : "0"; }
    }

    public static bool Interpolation
    {
        get { return ini["Graphics"]["Interpolation"].Equals("1") ? true : false; }
        set
        {
            ini["Graphics"]["Interpolation"] = value ? "1" : "0";
            ProjectSettings.SetSetting("physics/common/physics_interpolation", value);
            tree.PhysicsInterpolation = value;
        }
    }

    public static bool DownButtonHint
    {
        get { return ini["Graphics"]["Down Button Hint"].Equals("1") ? true : false; }
        set
        {
            ini["Graphics"]["Down Button Hint"] = value ? "1" : "0";
            var game = tree.Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame");
            if (game != null)
            {
                if (game.Juni.OnShift) { game.Juni.showShiftHint(true, jump_hint: false); }
                if (game.Juni.OnPlatform) { game.Juni.showShiftHint(true, jump_hint: true); }
            }
        }
    }

    public static bool UmbrellaCheat
    {
        get { return ini["TouchPanel"]["UmbrellaCheat"].Equals("1"); }
        set { ini["TouchPanel"]["UmbrellaCheat"] = value ? "1" : "0"; }
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

    public static bool ClassicDoubleJumpSound
    {
        get { return ini["Audio"]["Classic Double Jump Sound"].Equals("1") ? true : false; }
        set { ini["Audio"]["Classic Double Jump Sound"] = value ? "1" : "0"; }
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

    public static string Saves => SavesDirectory == "" ? GDKnyttDataStore.BaseDataDirectory.PathJoin("Saves") : SavesDirectory;

    public static bool LeftStickMovement
    {
        get { return ini["Misc"]["Left Stick Movement"].Equals("1") ? true : false; }
        set { ini["Misc"]["Left Stick Movement"] = value ? "1" : "0"; }
    }

    public static float StickSensitivity
    {
        get { return float.Parse(ini["Misc"]["Stick Sensitivity"]); }
        set { ini["Misc"]["Stick Sensitivity"] = value.ToString(); GDKnyttKeys.StickTreshold = 1 - value; }
    }

    public static string Version
    {
        get { return ini["Misc"]["Version"]; }
    }

    public override void _Ready()
    {
        tree = GetTree();
        initialize();
    }

    private static void moveUserDir()
    {
        if (OS.GetName() == "Windows" || OS.GetName() == "Linux")
        {
            string prev_data_dir = OS.GetUserDataDir().PathJoin("../godot/app_userdata/YKnytt");
            if (System.IO.File.Exists(prev_data_dir.PathJoin("input.ini")) && !FileAccess.FileExists("user://input.ini"))
            {
                System.IO.File.Move(prev_data_dir.PathJoin("input.ini"), OS.GetUserDataDir().PathJoin("input.ini"));
            }
            if (System.IO.File.Exists(prev_data_dir.PathJoin("settings.ini")) && !FileAccess.FileExists("user://settings.ini"))
            {
                System.IO.File.Move(prev_data_dir.PathJoin("settings.ini"), OS.GetUserDataDir().PathJoin("settings.ini"));
            }
            if (System.IO.Directory.Exists(prev_data_dir.PathJoin("Saves")) && !DirAccess.DirExistsAbsolute("user://Saves"))
            {
                System.IO.Directory.Move(prev_data_dir.PathJoin("Saves"), OS.GetUserDataDir().PathJoin("Saves"));
            }
            if (System.IO.Directory.Exists(prev_data_dir.PathJoin("Worlds")) && !DirAccess.DirExistsAbsolute("user://Worlds"))
            {
                System.IO.Directory.Move(prev_data_dir.PathJoin("Worlds"), OS.GetUserDataDir().PathJoin("Worlds"));
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
        modified |= ensureSetting("Graphics", "Seamless", "0");
        modified |= ensureSetting("Graphics", "Aspect", (0.4f).ToString());
        modified |= ensureSetting("Graphics", "Border", Mobile && TouchSettings.isHandsOverlapping() ? "1" : "0");
        modified |= ensureSetting("Graphics", "Forced Map", "1");
        modified |= ensureSetting("Graphics", "Detailed Map", "1");
        modified |= ensureSetting("Graphics", "Shader Type", ShaderType.NoShader.ToString());
        modified |= ensureSetting("Graphics", "WSOD", "1");
        modified |= ensureSetting("Graphics", "Interpolation", "0");
        modified |= ensureSetting("Graphics", "Down Button Hint", "1");
        modified |= ensureSetting("TouchPanel", "UmbrellaCheat", "0");

        modified |= ensureSetting("Audio", "Master Volume", "100");
        modified |= ensureSetting("Audio", "Music Volume", "80");
        modified |= ensureSetting("Audio", "Environment Volume", "80");
        modified |= ensureSetting("Audio", "Effects Volume", "70");
        modified |= ensureSetting("Audio", "Effects Panning", "1");
        modified |= ensureSetting("Audio", "Classic Double Jump Sound", "1");

        modified |= ensureSetting("Server", "URL", "https://yknytt.pythonanywhere.com");
        modified |= ensureSetting("Server", "Connection", "Online");

        modified |= ensureSetting("Directories", "Worlds", "");
        modified |= ensureSetting("Directories", "Saves", "");
        modified |= ensureSetting("Directories", "For Download", "0");

        modified |= ensureSetting("Misc", "Stick Sensitivity", (0.7f).ToString());
        modified |= ensureSetting("Misc", "Left Stick Movement", "1");

        modified |= TouchSettings.ensureSettings();

        if (modified) { saveSettings(); }
        applyAllSettings();
    }

    private static void applyAllSettings()
    {
        Fullscreen = ini["Graphics"]["Fullscreen"].Equals("1") ? true : false;
        SmoothScaling = ini["Graphics"]["Smooth Scaling"].Equals("1") ? true : false;
        Aspect = floatParse(ini["Graphics"]["Aspect"]);
        SeamlessScroll = ini["Graphics"]["Seamless"].Equals("1") ? true : false;
        Border = ini["Graphics"]["Border"].Equals("1") ? true : false;
        ForcedMap = ini["Graphics"]["Forced Map"].Equals("1") ? true : false;
        DetailedMap = ini["Graphics"]["Detailed Map"].Equals("1") ? true : false;
        Shader = Enum.TryParse<ShaderType>(ini["Graphics"]["Shader Type"], out var f) ? f : ShaderType.NoShader;
        WSOD = ini["Graphics"]["WSOD"].Equals("1") ? true : false;
        Interpolation = ini["Graphics"]["Interpolation"].Equals("1") ? true : false;
        DownButtonHint = ini["Graphics"]["Down Button Hint"].Equals("1") ? true : false;
        MasterVolume = int.Parse(ini["Audio"]["Master Volume"]);
        MusicVolume = int.Parse(ini["Audio"]["Music Volume"]);
        EnvironmentVolume = int.Parse(ini["Audio"]["Environment Volume"]);
        EffectsVolume = int.Parse(ini["Audio"]["Effects Volume"]);
        EffectsPanning = floatParse(ini["Audio"]["Effects Panning"]);
        ClassicDoubleJumpSound = ini["Audio"]["Classic Double Jump Sound"].Equals("1") ? true : false;
        StickSensitivity = floatParse(ini["Misc"]["Stick Sensitivity"]);
        LeftStickMovement = ini["Misc"]["Left Stick Movement"].Equals("1") ? true : false;
    }

    private static float floatParse(string s)
    {
        var sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        return float.Parse(s.Replace(".", sep).Replace(",", sep));
    }

    public static void saveSettings()
    {
        var text = ini.ToString();
        using var f = FileAccess.Open(GDKnyttDataStore.BaseDataDirectory.PathJoin("settings.ini"), FileAccess.ModeFlags.Write);
        f.StoreString(text);
    }

    // returns true if error
    public static bool loadSettings()
    {
        var path = GDKnyttDataStore.BaseDataDirectory.PathJoin("settings.ini");
        if (!FileAccess.FileExists(path)) { return true; }
        using var f = FileAccess.Open(path, FileAccess.ModeFlags.Read);
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

    private static bool checkVersion()
    {
        if (!ini.Sections.ContainsSection("Misc")) { ini.Sections.AddSection("Misc"); }

        if (ini["Misc"].ContainsKey("Version") && ini["Misc"]["Version"].CompareTo("0.6.8") == -1)
        {
            ini["Misc"]["Version"] = "0.7.0";
            ini["Server"]?.RemoveKey("URL");
            if (OS.GetName() == "HTML5") { ini["Graphics"]?.RemoveKey("Detailed Map"); }
            
            if (FileAccess.FileExists(GDKnyttDataStore.BaseDataDirectory.PathJoin("input.ini")))
            {
                GDKnyttKeys.loadSettings();
                if (GDKnyttKeys.ini["Input"]["hologram1"] == "Joy(0)" &&
                    !GDKnyttKeys.ini["Input"].Any(k => k.Value == "Joy(1)"))
                {
                    GDKnyttKeys.ini["Input"]["hologram1"] = "Joy(1)";
                    GDKnyttKeys.saveSettings();
                }
            }
            return true;
        }
        if (!ini["Misc"].ContainsKey("Version") || ini["Misc"]["Version"].CompareTo("0.7.0") == -1)
        {
            ini["Misc"]["Version"] = "0.7.0";
            return true;
        }
        return false;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("fullscreen")) { Fullscreen = !Fullscreen; }

        if (Input.IsActionJustPressed("border"))
        {
            Border = !Border;
            saveSettings();
        }

        if (Input.IsActionJustPressed("zoom") && ! Fullscreen)
        {
            Vector2 max_size = DisplayServer.ScreenGetSize(), cur_size = DisplayServer.WindowGetSize(),
                min_size = new Vector2((int)ProjectSettings.GetSetting("display/window/size/width"), 
                                       (int)ProjectSettings.GetSetting("display/window/size/height"));
            int scale = (int)Mathf.Min(cur_size.X / min_size.X, cur_size.Y / min_size.Y);
            int max_scale = (int)Mathf.Min(max_size.X / min_size.X, max_size.Y / min_size.Y);
            
            scale++;
            if (scale > max_scale) { scale = 1; }
            DisplayServer.WindowGetSize() = min_size * scale;
            DisplayServer.WindowGetPosition() = (max_size - DisplayServer.WindowGetSize()) / 2;
            DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Maximized = false;
        }
    }
}
