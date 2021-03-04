using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GDKnyttDataStore : Node
{
    public static Random random = new Random();
    public static GDKnyttWorldImpl KWorld { get; set; }

    private static SceneTree Tree { get; set; }

    public static string CutsceneName { get; private set; }
    public static string CutsceneAfter { get; private set; }
    public static Node CutsceneReturn { get; private set; }

    public enum CutsceneMode { Intro, Middle, Ending };
    public static CutsceneMode Mode { get; private set; }
    public static string CutsceneSound { get; private set; }

    public override void _Ready()
    {
        Tree = GetTree();
    }

    public static void startGame(bool new_game)
    {
        if (new_game)
        {
            Mode = CutsceneMode.Intro;
            startCutscene("Intro", "res://knytt/GDKnyttGame.tscn");
        }
        else { Tree.ChangeScene("res://knytt/GDKnyttGame.tscn"); }
    }

    public static void winGame(string ending = "Ending")
    {
        Mode = CutsceneMode.Ending;
        startCutscene(ending, "res://knytt/ui/MainMenu.tscn");
    }

    public static void playCutscene(string cutscene, string sound)
    {
        Mode = CutsceneMode.Middle;
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
        // TODO: how not to blink?
        Tree.ChangeScene("res://knytt/ui/Cutscene.tscn");
    }

    private static void startCutscene(string cutscene, string after)
    {
        CutsceneName = cutscene;
        CutsceneAfter = after;
        CutsceneReturn = null;
        CutsceneSound = null;
        if (!KWorld.worldFileExists(Cutscene.makeScenePath(1))) { Tree.ChangeScene(after); return; }
        Tree.ChangeScene("res://knytt/ui/Cutscene.tscn");
    }
}

public static class RandomExtension
{
    public static T NextElement<T>(this Random random, ICollection<T> e)
    {
        return e.ElementAt(GDKnyttDataStore.random.Next(e.Count()));
    }
}
