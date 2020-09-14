using System;
using Godot;

public class GDKnyttDataStore : Node
{
    public static Random random = new Random();
    public static GDKnyttWorldImpl KWorld { get; set; }

    private static SceneTree Tree { get; set; }

    public static string CutsceneName { get; private set; }
    public static string CutsceneAfter { get; private set; }

    public override void _Ready()
    {
        Tree = GetTree();
    }

    public static void startGame(bool new_game)
    {
        if (new_game) { startCutscene("Intro", "res://knytt/GDKnyttGame.tscn"); }
        else { Tree.ChangeScene("res://knytt/GDKnyttGame.tscn"); }
    }

    public static void winGame(string ending="Ending")
    {
        startCutscene(ending, "res://knytt/ui/MainMenu.tscn");
    }

    private static void startCutscene(string cutscene, string after)
    {
        CutsceneName = cutscene;
        CutsceneAfter = after;
        GD.Print(Cutscene.makeScenePath(1));
        if (!KWorld.worldFileExists(Cutscene.makeScenePath(1))) { Tree.ChangeScene(after); return; }
        Tree.ChangeScene("res://knytt/ui/Cutscene.tscn");
    }
}
