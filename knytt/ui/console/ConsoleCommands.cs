using Godot;
using System;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using YKnyttLib;
using static YKnyttLib.JuniValues;

/// <summary>
/// Registers game-specific commands with LimboConsole.
/// To add a new command, just write a public method and tag it:
///
///   [Cmd("greet", "Say hello")]
///   public void CmdGreet(string name) => Info($"Hello, {name}!");
///
/// Registration is automatic via reflection; arguments come from the method signature.
/// Use spaces in the name for subcommands: "save copy", "mon flags".
/// </summary>
public partial class ConsoleCommands : Node
{
    private Node _limbo;

    // Monitor state
    private string _monMode; // null=off, ""=continuous, "flash"=on change, "flags"=with flags
    private string _lastMonLine;

    public override void _Ready()
    {
        _limbo = GetNode("/root/LimboConsole");
        SetPhysicsProcess(false);

        // Auto-register all methods tagged with [Cmd]
        foreach (var method in GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            var attr = method.GetCustomAttribute<CmdAttribute>();
            if (attr == null) continue;
            _limbo.Call("register_command", new Callable(this, method.Name), attr.Name, attr.Desc);
        }
    }

    #region Helpers

    private GDKnyttGame Game => GetTree().Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame");
    private Juni Juni => Game?.Juni;

    private void Info(string msg) => _limbo.Call("info", msg);
    private void Warn(string msg) => _limbo.Call("warn", msg);
    private void Error(string msg) => _limbo.Call("error", msg);

    private bool RequireGame()
    {
        if (Game != null) return true;
        Error("No game is running.");
        return false;
    }

    private string PowersString() => string.Join("", Juni.Powers.Powers.Select(p => p ? "1" : "0"));
    private string FlagsString() => string.Join("", Juni.Powers.Flags.Select(f => f ? "1" : "0"));

    #endregion

    #region Speed

    [Cmd("speed", "Print current game speed")]
    public void CmdSpeed() => Info($"Speed: {GDKnyttDataStore.CurrentSpeed:F1}x");

    [Cmd("speed set", "Set game speed (0.1-5.0)")]
    public void CmdSpeedSet(float value)
    {
        GDKnyttDataStore.CurrentSpeed = Mathf.Clamp(value, 0.1f, 5.0f);
        Info($"Speed: {GDKnyttDataStore.CurrentSpeed:F1}x");
    }

    #endregion

    #region Save

    [Cmd("save", "Save at current position")]
    public void CmdSave()
    {
        if (!RequireGame()) return;
        Game.saveGame(Juni, true);
        Info("Game saved.");
    }

    [Cmd("save print", "Print save file contents")]
    public void CmdSavePrint()
    {
        if (!RequireGame()) return;
        Info(Game.GDWorld.KWorld.CurrentSave.ToString());
    }

    [Cmd("save copy", "Copy save to clipboard")]
    public void CmdSaveCopy()
    {
        if (!RequireGame()) return;
        DisplayServer.ClipboardSet(Game.GDWorld.KWorld.CurrentSave.ToString());
        Info("Save copied to clipboard.");
    }

    [Cmd("save paste", "Load save from clipboard")]
    public void CmdSavePaste()
    {
        if (!RequireGame()) return;
        var text = DisplayServer.ClipboardGet();
        if (string.IsNullOrWhiteSpace(text)) { Error("Clipboard is empty."); return; }
        Game.saveGame(new KnyttSave(Game.GDWorld.KWorld, text, 0), save_map: false);
        Info("Save loaded from clipboard. Use 'reboot' to apply.");
    }

    [Cmd("save backup", "Zip all save files to Documents")]
    public void CmdSaveBackup()
    {
        bool web = GDKnyttSettings.Web;
        var dest_path = OS.GetSystemDir(OS.SystemDir.Documents);
        var dest_zip = dest_path.PathJoin("yknytt_saves.zip");

        if (!DirAccess.DirExistsAbsolute(dest_path)) { Error($"Directory {dest_path} does not exist."); return; }
        if (System.IO.File.Exists(dest_zip) && web) { System.IO.File.Delete(dest_zip); }
        if (FileAccess.FileExists(dest_zip)) { Error($"{dest_zip} already exists."); return; }

        try
        {
            ZipFile.CreateFromDirectory(GDKnyttSettings.Saves, dest_zip);
        }
        catch (Exception)
        {
            Error($"Cannot create zip. Please check that you have access to {dest_path}." +
                  (OS.GetName() == "Android" ?
                   " Grant permissions in Settings -> Apps -> YKnytt -> Permissions -> Storage." : ""));
            return;
        }

        if (web) { JavaScriptBridge.DownloadBuffer(GDKnyttAssetManager.loadFile(dest_zip), "yknytt_saves.zip"); }
        Info($"{dest_zip} was successfully created.");
    }

    [Cmd("save data", "Export working directory (with levels in web, without them otherwise)")]
    public void CmdSaveData()
    {
        bool web = GDKnyttSettings.Web;
        var dest_path = OS.GetSystemDir(OS.SystemDir.Documents);
        var dest_zip = dest_path.PathJoin("yknytt_data.zip");

        if (!DirAccess.DirExistsAbsolute(dest_path)) { Error($"Directory {dest_path} does not exist."); return; }
        if (System.IO.File.Exists(dest_zip) && web) { System.IO.File.Delete(dest_zip); }
        if (FileAccess.FileExists(dest_zip)) { Error($"{dest_zip} already exists."); return; }

        try
        {
            if (web)
            {
                ZipFile.CreateFromDirectory(GDKnyttDataStore.BaseDataDirectory, dest_zip);
                JavaScriptBridge.DownloadBuffer(GDKnyttAssetManager.loadFile(dest_zip), "yknytt_data.zip");
            }
            else
            {
                createDataZipWithNoWorlds(dest_zip);
            }
        }
        catch (Exception)
        {
            Error($"Cannot create zip. Maybe some file is locked, or you don't have access to {dest_path}.");
            return;
        }

        Info($"{dest_zip} was successfully created. " +
             "Unpack it to Documents/yknytt_data on your device to continue playing.");
    }

    private void createDataZipWithNoWorlds(string dest_zip)
    {
        const string exclude_path = "Worlds";
        using (ZipArchive zip = ZipFile.Open(dest_zip, ZipArchiveMode.Create))
        {
            var base_dir = GDKnyttDataStore.BaseDataDirectory;
            foreach (var file in System.IO.Directory.GetFiles(base_dir, "*", System.IO.SearchOption.AllDirectories))
            {
                var full_path = System.IO.Path.GetFullPath(file);
                var relative_path = full_path.Substring(base_dir.Length)
                                             .TrimStart(System.IO.Path.DirectorySeparatorChar);
                if (relative_path.StartsWith(exclude_path + System.IO.Path.DirectorySeparatorChar)) { continue; }
                zip.CreateEntryFromFile(full_path, relative_path, CompressionLevel.Optimal);
            }
        }
    }

    #endregion

    #region Password

    [Cmd("pass", "Print compressed password")]
    public void CmdPass()
    {
        if (!RequireGame()) return;
        Game.saveGame(Juni, false);
        Info(KnyttUtil.CompressString(Game.GDWorld.KWorld.CurrentSave.ToString()));
    }

    [Cmd("pass copy", "Copy password to clipboard")]
    public void CmdPassCopy()
    {
        if (!RequireGame()) return;
        Game.saveGame(Juni, false);
        DisplayServer.ClipboardSet(KnyttUtil.CompressString(Game.GDWorld.KWorld.CurrentSave.ToString()));
        Info("Password copied to clipboard.");
    }

    [Cmd("pass paste", "Load password from clipboard")]
    public void CmdPassPaste()
    {
        if (!RequireGame()) return;
        var text = DisplayServer.ClipboardGet();
        if (string.IsNullOrWhiteSpace(text)) { Error("Clipboard is empty."); return; }
        try
        {
            var save = new KnyttSave(Game.GDWorld.KWorld, KnyttUtil.DecompressString(text), 0);
            Game.saveGame(save, save_map: false);
            Info("Password loaded. Use 'reboot' to apply.");
        }
        catch (Exception e) { Error($"Invalid password: {e.Message}"); }
    }

    #endregion

    #region Powers & Flags

    private static readonly (string name, PowerNames power)[] PowerMap = {
        ("run",           PowerNames.Run),
        ("climb",         PowerNames.Climb),
        ("doublejump",    PowerNames.DoubleJump),
        ("highjump",      PowerNames.HighJump),
        ("eye",           PowerNames.Eye),
        ("enemydetector", PowerNames.EnemyDetector),
        ("umbrella",      PowerNames.Umbrella),
        ("hologram",      PowerNames.Hologram),
        ("redkey",        PowerNames.RedKey),
        ("yellowkey",     PowerNames.YellowKey),
        ("bluekey",       PowerNames.BlueKey),
        ("purplekey",     PowerNames.PurpleKey),
        ("map",           PowerNames.Map),
    };

    [Cmd("set", "Set a power or flag (set <name> on/off)")]
    public void CmdSet(string power, string value)
    {
        if (!RequireGame()) return;

        bool? boolVal = value.ToLower() switch
        {
            "on" or "1" or "true" => true,
            "off" or "0" or "false" => false,
            _ => null,
        };

        // Bulk powers: "set powers 1100000000000"
        if (power.ToLower() == "powers" && value.Length == 13)
        {
            for (int i = 0; i < 13; i++) Juni.setPower(i, value[i] == '1');
            Game.sendCheat();
            Info($"Powers: {PowersString()}");
            return;
        }

        // Bulk flags: "set flags 0000000000"
        if (power.ToLower() == "flags" && value.Length == 10)
        {
            for (int i = 0; i < 10; i++) Juni.Powers.setFlag(i, value[i] == '1');
            Game.sendCheat();
            Info($"Flags: {FlagsString()}");
            return;
        }

        if (boolVal == null) { Error("Value must be on/off/1/0."); return; }

        // Flags: flag0-flag9
        if (power.ToLower().StartsWith("flag") && int.TryParse(power[4..], out int fi) && fi is >= 0 and <= 9)
        {
            Juni.Powers.setFlag(fi, boolVal.Value);
            Game.sendCheat();
            Info($"flag{fi} = {(boolVal.Value ? "on" : "off")}");
            return;
        }

        // Individual power
        foreach (var (name, pn) in PowerMap)
        {
            if (string.Equals(power, name, StringComparison.OrdinalIgnoreCase))
            {
                Juni.setPower(pn, boolVal.Value);
                Game.sendCheat();
                Info($"{name} = {(boolVal.Value ? "on" : "off")}");
                return;
            }
        }

        Error($"Unknown power '{power}'. Valid: {string.Join(" ", PowerMap.Select(p => p.name))}, flag0-flag9");
    }

    #endregion

    #region Monitor

    [Cmd("mon", "Monitor Juni position (continuous)")]
    public void CmdMon() => StartMon("");

    [Cmd("mon flash", "Monitor only on change")]
    public void CmdMonFlash() => StartMon("flash");

    [Cmd("mon flags", "Monitor with flags")]
    public void CmdMonFlags() => StartMon("flags");

    [Cmd("mon off", "Disable monitor")]
    public void CmdMonOff() { _monMode = null; SetPhysicsProcess(false); Info("Monitor off."); }

    private void StartMon(string mode)
    {
        if (!RequireGame()) return;
        _monMode = mode;
        _lastMonLine = null;
        SetPhysicsProcess(true);
        Info($"Monitor on ({(mode == "" ? "continuous" : mode)}).");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_monMode == null || Juni == null) return;

        var area = Juni.GDArea.Area.Position;
        var pos = Juni.AreaPosition;
        var line = $"Area({area.X},{area.Y}) Pos({pos.X},{pos.Y})";
        if (_monMode == "flags") line += $" P:{PowersString()} F:{FlagsString()}";
        if (_monMode == "flash" && line == _lastMonLine) return;

        _lastMonLine = line;
        Info(line);
    }

    #endregion

    #region Cheats

    [Cmd("idclip", "Toggle no-clip mode")]
    public void CmdIdclip()
    {
        if (!RequireGame()) return;
        Juni.DebugFlyMode = !Juni.DebugFlyMode;
        Info($"No-clip {(Juni.DebugFlyMode ? "on" : "off")}.");
    }

    [Cmd("iddqd", "Toggle invulnerability")]
    public void CmdIddqd()
    {
        if (!RequireGame()) return;
        Juni.Immune = !Juni.Immune;
        Info($"God mode {(Juni.Immune ? "on" : "off")}.");
    }

    [Cmd("map", "Toggle force map")]
    public void CmdMap()
    {
        if (!RequireGame()) return;
        Game.UI.ForceMap = !Game.UI.ForceMap;
        Info($"Map {(Game.UI.ForceMap ? "on" : "off")}.");
    }

    #endregion

    #region Teleportation

    [Cmd("shift", "Shift Juni relative (xMap yMap)")]
    public void CmdShift(int xMap, int yMap) => DoTeleport(xMap, yMap, null, null, absolute: false);

    [Cmd("shift pos", "Shift Juni relative with position")]
    public void CmdShiftPos(int xMap, int yMap, int xPos, int yPos) => DoTeleport(xMap, yMap, xPos, yPos, absolute: false);

    [Cmd("goto", "Teleport Juni absolute (xMap yMap)")]
    public void CmdGoto(int xMap, int yMap) => DoTeleport(xMap, yMap, null, null, absolute: true);

    [Cmd("goto pos", "Teleport Juni absolute with position")]
    public void CmdGotoPos(int xMap, int yMap, int xPos, int yPos) => DoTeleport(xMap, yMap, xPos, yPos, absolute: true);

    private void DoTeleport(int xMap, int yMap, int? xPos, int? yPos, bool absolute)
    {
        if (!RequireGame()) return;

        var shift = new KnyttShift(Game.CurrentArea.Area.Position, Juni.AreaPosition, KnyttSwitch.SwitchID.A);
        shift.AbsoluteTarget = absolute;
        shift.FormattedArea = new KnyttPoint(xMap, yMap);
        if (xPos != null && yPos != null)
            shift.FormattedPosition = new KnyttPoint(xPos.Value, yPos.Value);
        else
            shift.RelativePosition = KnyttPoint.Zero;

        if (!shift.RelativeArea.isZero()) { Game.changeAreaDelta(shift.RelativeArea); }
        Game.translateCamera();
        Juni.moveToPosition(Game.CurrentArea, shift.AbsolutePosition);
        Game.sendCheat();

        var dest = absolute ? $"({xMap},{yMap})" : $"+({xMap},{yMap})";
        Info($"Teleported to {dest}.");
    }

    #endregion

    #region Debug Visuals

    [Cmd("col", "Toggle collision shape display")]
    public void CmdCol()
    {
        GetTree().DebugCollisionsHint = !GetTree().DebugCollisionsHint;
        Info($"Collisions {(GetTree().DebugCollisionsHint ? "on" : "off")}.");
    }

    [Cmd("trail", "Toggle movement trails")]
    public void CmdTrail()
    {
        if (!RequireGame()) return;
        Game.Trails.On = !Game.Trails.On;
        Info($"Trails {(Game.Trails.On ? "on" : "off")} (count={Game.Trails.TrailCount}, frames={Game.Trails.TrailFrames}).");
    }

    [Cmd("trail set", "Set trail count and frames, then toggle")]
    public void CmdTrailSet(int count, int frames)
    {
        if (!RequireGame()) return;
        Game.Trails.TrailCount = count;
        Game.Trails.TrailFrames = frames;
        Game.Trails.On = !Game.Trails.On;
        Info($"Trails {(Game.Trails.On ? "on" : "off")} (count={count}, frames={frames}).");
    }

    [Cmd("death", "Toggle death markers")]
    public void CmdDeath()
    {
        if (!RequireGame()) return;
        Game.Deaths.On = !Game.Deaths.On;
        Info($"Death markers {(Game.Deaths.On ? "on" : "off")}.");
    }

    #endregion

    #region World

    [Cmd("reboot", "Clear cache and re-read this world")]
    public void CmdReboot()
    {
        if (!RequireGame()) return;
        Game.GDWorld.KWorld.refreshWorld(tilesets_only: false);
        GDKnyttDataStore.startGame(false);
    }

    [Cmd("reboot tilesets", "Clear only compiled tilesets and re-read this world")]
    public void CmdRebootTilesets()
    {
        if (!RequireGame()) return;
        Game.GDWorld.KWorld.refreshWorld(tilesets_only: true);
        GDKnyttDataStore.startGame(false);
    }

    [Cmd("world", "Load a .knytt.bin by file name (bundled worlds, then the Worlds directory)")]
    public void CmdWorld(string world) => loadWorldByName(world, intro: false);

    [Cmd("world intro", "Load a .knytt.bin and play its intro cutscene")]
    public void CmdWorldIntro(string world) => loadWorldByName(world, intro: true);

    private void loadWorldByName(string world, bool intro)
    {
        string path = null;
        string internal_path = $"res://knytt/worlds/{world}";
        if (FileAccess.FileExists(internal_path)) { path = internal_path; }
        else
        {
            string external_path = GDKnyttDataStore.BaseDataDirectory.PathJoin($"Worlds/{world}");
            if (FileAccess.FileExists(external_path)) { path = external_path; }
        }
        if (path == null) { Error($"Cannot find world: \"{world}\""); return; }

        try
        {
            var binloader = new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile(path));
            var kworld = new GDKnyttWorldImpl();
            kworld.setDirectory(path, binloader.RootDirectory);
            kworld.loadWorldConfig(GDKnyttAssetManager.loadTextFileRaw(binloader.GetFile("World.ini")));
            kworld.CurrentSave = new KnyttSave(kworld,
                GDKnyttAssetManager.loadTextFile(binloader.GetFile("DefaultSavegame.ini")), 1);
            kworld.setBinMode(binloader);
            GDKnyttDataStore.KWorld = kworld;
            Info($"World loaded: {path}");
            GDKnyttDataStore.startGame(intro);
        }
        catch (Exception e) { Error($"Cannot load {path}: {e.Message}"); }
    }

    // LimboConsole's "quit" exits the game; the old console used quit/exit to close it.
    // limbo_console.cfg points the "exit" alias here.
    [Cmd("hide", "Hide this console")]
    public void CmdHide() => GetNode<Console>("/root/Console").toggleConsole();

    [Cmd("youtube", "Search YouTube for this world")]
    public void CmdYoutube()
    {
        if (!RequireGame()) return;
        var name = Game.GDWorld.KWorld.Info.Name.Replace(" ", "+");
        OS.ShellOpen($"https://www.youtube.com/results?search_query=Knytt+Stories+{name}");
        Info($"Searching YouTube for '{Game.GDWorld.KWorld.Info.Name}'.");
    }

    [Cmd("hell", "Show stats + teleport to hardest place")]
    public void CmdHell()
    {
        if (!RequireGame()) return;
        var p = Juni.Powers;
        Info($"Time: {p.TotalTimeNow}s | Deaths: {p.TotalDeaths} | Hardest: {p.HardestPlace} ({p.HardestPlaceDeaths}d)");

        if (string.IsNullOrEmpty(p.HardestPlace)) { Warn("No deaths recorded yet."); return; }

        // Parse "x999y1000:x5y8" format -> teleport there
        try
        {
            var parts = p.HardestPlace.Split(':');
            int ax = int.Parse(parts[0].Split('y')[0][1..]);
            int ay = int.Parse(parts[0].Split('y')[1]);
            int px = parts.Length > 1 ? int.Parse(parts[1].Split('y')[0][1..]) : 0;
            int py = parts.Length > 1 ? int.Parse(parts[1].Split('y')[1]) : 0;
            DoTeleport(ax, ay, px, py, absolute: true);
        }
        catch { Error($"Could not parse location '{p.HardestPlace}'."); }
    }

    #endregion

    #region Settings

    [Cmd("settings", "Print all settings")]
    public void CmdSettingsPrint() => Info(GDKnyttSettings.ini.ToString());

    [Cmd("settings copy", "Copy settings to clipboard")]
    public void CmdSettingsCopy()
    {
        DisplayServer.ClipboardSet(GDKnyttSettings.ini.ToString());
        Info("Settings copied to clipboard.");
    }

    [Cmd("settings set", "Set a setting (section name value, '_' for spaces, value 'delete' removes it)")]
    public void CmdSettingsSet(string section, string name, string value)
    {
        if (!GDKnyttSettings.ini.Sections.ContainsSection(section)) { Error("Wrong section name!"); return; }
        if (section != "Physics") { name = name.Replace('_', ' '); }
        if (value == "delete") { GDKnyttSettings.ini[section]?.RemoveKey(name); }
        else { GDKnyttSettings.ini[section][name] = value; }
        GDKnyttSettings.applyAllSettings();
        GDKnyttSettings.saveSettings();
        Info(value != "delete" ? $"[{section}] {name} = {value}" : $"[{section}] {name} has been deleted.");
    }

    #endregion
}

/// <summary>
/// Tag a public method to auto-register it as a console command.
/// Name supports subcommands via spaces: "save copy", "mon flags".
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CmdAttribute : Attribute
{
    public string Name { get; }
    public string Desc { get; }
    public CmdAttribute(string name, string desc) { Name = name; Desc = desc; }
}
