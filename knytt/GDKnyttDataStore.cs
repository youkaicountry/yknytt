using System;
using Godot;

public class GDKnyttDataStore : Node
{
    public static Random random = new Random();
    public static GDKnyttWorldImpl KWorld { get; set; }

    private static SceneTree Tree { get; set; }

    public override void _Ready()
    {
        Tree = GetTree();
    }

    public static void startGame()
    {
        Tree.ChangeScene("res://knytt/GDKnyttGame.tscn");
    }

    public static void winGame()
    {
        Tree.ChangeScene("res://knytt/ui/MainMenu.tscn");
    }
}
