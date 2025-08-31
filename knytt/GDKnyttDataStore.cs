using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GDKnyttDataStore : Node
{
    public static Random random = new Random();
    public static GDKnyttWorldImpl KWorld { get; set; }

    public static SceneTree Tree { get; set; }

    public static string ProgressHint { get; set; }

    public static string BaseDataDirectory { get; private set; }
    
    static GDKnyttDataStore()
    {
        // Initialize base data directory, checking for command line arguments
        BaseDataDirectory = OS.GetUserDataDir();
        
        // Check for --data command line parameter
        var args = OS.GetCmdlineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--data" && i + 1 < args.Length)
            {
                string dataPath = args[i + 1];
                // Convert relative paths to absolute paths
                if (!dataPath.IsAbsPath())
                {
                    dataPath = OS.GetExecutablePath().GetBaseDir().PlusFile(dataPath);
                }
                BaseDataDirectory = dataPath;
                GD.Print($"Using custom data directory: {BaseDataDirectory}");
                break;
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
        get { return Godot.Engine.TimeScale; }
        set
        {
            int iterations = (int)(BaseIterations * value);
            Godot.Engine.TimeScale = value;
            Godot.Engine.IterationsPerSecond = iterations;
        }
    }

    public override void _Ready()
    {
        Tree = GetTree();
    }

    public static void startGame(bool new_game)
    {
        Tree.Paused = false;
        if (new_game)
        {
            Mode = CutsceneMode.Intro;
            CutsceneFadeIn = true;
            startCutscene("Intro", "res://knytt/GDKnyttGame.tscn");
        }
        else { Tree.ChangeScene("res://knytt/GDKnyttGame.tscn"); }
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
        if (!KWorld.worldFileExists(Cutscene.makeScenePath(1))) { Tree.ChangeScene(after); return; }
        GDKnyttSettings.setupViewport(for_ui: true);
        Tree.ChangeScene("res://knytt/ui/Cutscene.tscn");
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
        Tree.ChangeScene("res://knytt/ui/Cutscene.tscn");
    }
}

public static class RandomExtension
{
    public static T NextElement<T>(this Random random, ICollection<T> e)
    {
        return e.ElementAt(random.Next(e.Count()));
    }
}
