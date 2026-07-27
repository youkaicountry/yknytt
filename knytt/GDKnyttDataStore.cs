using Godot;
using YKnyttLib.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GDKnyttDataStore : Node
{
    public static Random random = new Random();
    public static GDKnyttWorldImpl KWorld { get; set; }

    public static SceneTree Tree { get; set; }

    public static string ProgressHint { get; set; }

    public static string BaseDataDirectory { get; private set; }
    
    public static string WorldsIni => GDKnyttSettings.WorldsDirectory == "" ?
        BaseDataDirectory.PathJoin("worlds.ini") :
        BaseDataDirectory.PathJoin($"worlds.{GDKnyttSettings.WorldsDirectory.Hash():X8}.ini");

    public static bool GptokeybMode { get; private set; }

    // ARM64 handheld console. Godot 3 detected these through the FRT platform, whose
    // OS.get_name() was "Unix"; FRT has no Godot 4 port, so it has to be declared:
    // --gptokeyb (what the PortMaster launcher already passes), an explicit --handheld,
    // or a "handheld" feature tag on a custom export preset.
    private static bool handheld_flag;
    public static bool Handheld => handheld_flag || GptokeybMode || OS.HasFeature("handheld");

    // Maintenance mode: rebuild the shipped default TileSets and exit. See
    // GDKnyttAssetManager.compileTileset().
    public static bool CompileTilesetsMode { get; private set; }

    static GDKnyttDataStore()
    {
        // Initialize base data directory, checking for command line arguments
        BaseDataDirectory = OS.GetUserDataDir();

        string base_dir_candidate = OS.GetSystemDir(OS.SystemDir.Documents).PathJoin("yknytt_data");
        if (DirAccess.DirExistsAbsolute(base_dir_candidate)) { BaseDataDirectory = base_dir_candidate; }
        
        // Check for --data command line parameter.
        // Godot 4 keeps anything after a bare "--" in GetCmdlineUserArgs(), so scan both.
        var engine_args = OS.GetCmdlineArgs();
        var user_args = OS.GetCmdlineUserArgs();
        var args = new string[engine_args.Length + user_args.Length];
        engine_args.CopyTo(args, 0);
        user_args.CopyTo(args, engine_args.Length);
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--data" && i + 1 < args.Length)
            {
                string dataPath = args[i + 1];
                // Convert relative paths to absolute paths
                // Godot 4: IsAbsPath() is no longer available as string extension
                if (!System.IO.Path.IsPathRooted(dataPath))
                {
                    dataPath = OS.GetExecutablePath().GetBaseDir().PathJoin(dataPath);
                }
                BaseDataDirectory = dataPath;
                GD.Print($"Using custom data directory: {BaseDataDirectory}");
            }

            if (args[i] == "--gptokeyb")
            {
                GptokeybMode = true;
            }

            if (args[i] == "--handheld")
            {
                handheld_flag = true;
            }

            if (args[i] == "--compile-tilesets")
            {
                CompileTilesetsMode = true;
            }
        }
    }

    public static string CutsceneName { get; private set; }
    public static string CutsceneAfter { get; private set; }
    public static Node CutsceneReturn { get; private set; }

    public enum CutsceneMode { Intro, Middle, Ending };
    public static CutsceneMode Mode { get; private set; }
    public static bool CutsceneFadeIn { get; private set; }
    public static string CutsceneSound { get; private set; }
    public static string Statistics { get; private set; }

    const int BaseIterations = 60;
    public static float CurrentSpeed
    {
        get { return (float)Godot.Engine.TimeScale; }
        set
        {
            int iterations = (int)(BaseIterations * value);
            Godot.Engine.TimeScale = value;
            // Godot 4: IterationsPerSecond → PhysicsTicksPerSecond
            Godot.Engine.PhysicsTicksPerSecond = iterations;
        }
    }

    public static void setTitle(string level_name)
    {
        // Godot 4: OS.SetWindowTitle → DisplayServer.WindowSetTitle
        DisplayServer.WindowSetTitle("YKnytt" + (level_name == null ? "" : $" | {level_name}"));
    }

    public override void _Ready()
    {
        Tree = GetTree();

        if (CompileTilesetsMode)
        {
            // Runs before the main scene, so nothing else has to load first. The Console
            // autoload registers the log targets and readies *after* this one, and we quit
            // before it ever does, so wire up output here or the run is silent.
            KnyttLogger.AddTarget(new GodotLoggerTarget());
            int count = GDKnyttAssetManager.compileTileset();
            Tree.Quit(count > 0 ? 0 : 1);
        }
    }

    public static void startGame(bool new_game)
    {
        Tree.Paused = false;
        setTitle(KWorld.WorldDirectoryName);
        if (new_game)
        {
            Mode = CutsceneMode.Intro;
            CutsceneFadeIn = true;
            startCutscene("Intro", "res://knytt/GDKnyttGame.tscn");
        }
        else { Tree.ChangeSceneToFile("res://knytt/GDKnyttGame.tscn"); }
    }

    public static void winGame(string ending, string statistics)
    {
        Mode = CutsceneMode.Ending;
        CurrentSpeed = 1;
        CutsceneFadeIn = false;
        Statistics = statistics;
        startCutscene(ending, "res://knytt/ui/MainMenu.tscn");
    }

    private static void startCutscene(string cutscene, string after)
    {
        CutsceneName = cutscene;
        CutsceneAfter = after;
        CutsceneReturn = null;
        CutsceneSound = null;
        if (!KWorld.worldFileExists(Cutscene.makeScenePath(1))) { Tree.ChangeSceneToFile(after); return; }
        GDKnyttSettings.setupViewport(for_ui: true);
        Tree.ChangeSceneToFile("res://knytt/ui/Cutscene.tscn");
    }

    public static void playCutscene(string cutscene, string sound)
    {
        Mode = CutsceneMode.Middle;
        CutsceneFadeIn = false;
        CutsceneName = cutscene;
        CutsceneAfter = null;
        CutsceneReturn = Tree.CurrentScene;
        CutsceneSound = sound;
        if (!KWorld.worldFileExists(Cutscene.makeScenePath(1)))
        {
            if (Tree.Paused) { Tree.Paused = false; Cutscene.releaseAll(); }
            return;
        }
        GDKnyttSettings.setupViewport(for_ui: true);
        Tree.Paused = true;
        Tree.Root.RemoveChild(Tree.CurrentScene);
        Tree.ChangeSceneToFile("res://knytt/ui/Cutscene.tscn");
    }
}

public static class RandomExtension
{
    public static T NextElement<T>(this Random random, ICollection<T> e)
    {
        return e.ElementAt(random.Next(e.Count()));
    }
}
