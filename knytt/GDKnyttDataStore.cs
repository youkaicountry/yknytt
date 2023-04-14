using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GDKnyttDataStore : Node
{
    public static Random random = new Random();
    public static GDKnyttWorldImpl KWorld { get; set; }

    private static SceneTree Tree { get; set; }

    public static string CutsceneName { get; private set; }
    public static string CutsceneAfter { get; private set; }
    public static Node CutsceneReturn { get; private set; }

    public enum CutsceneMode { Intro, Middle, Ending };
    public static CutsceneMode Mode { get; private set; }
    public static bool CutsceneFadeIn { get; private set; }
    public static string CutsceneSound { get; private set; }

    const int BaseIterations = 60;
    public static double CurrentSpeed
    {
        get { return Godot.Engine.TimeScale; }
        set
        {
            int iterations = (int)(BaseIterations * value);
            Godot.Engine.TimeScale = value;
            Godot.Engine.PhysicsTicksPerSecond = iterations;
        }
    }

    public override void _Ready()
    {
        Tree = GetTree();
    }

    public static void startGame(bool new_game)
    {
        if (new_game)
        {
            Mode = CutsceneMode.Intro;
            CutsceneFadeIn = true;
            startCutscene("Intro", "res://knytt/GDKnyttGame.tscn");
        }
        else { Tree.ChangeSceneToFile("res://knytt/GDKnyttGame.tscn"); }
    }

    public static void winGame(string ending = "Ending")
    {
        Mode = CutsceneMode.Ending;
        CutsceneFadeIn = false;
        startCutscene(ending, "res://knytt/ui/MainMenu.tscn");
    }

    private static void startCutscene(string cutscene, string after)
    {
        CutsceneName = cutscene;
        CutsceneAfter = after;
        CutsceneReturn = null;
        CutsceneSound = null;
        if (!KWorld.worldFileExists(Cutscene.makeScenePath(1))) { Tree.ChangeSceneToFile(after); return; }
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
