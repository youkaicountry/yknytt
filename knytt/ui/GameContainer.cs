using Godot;
using YKnyttLib;

public class GameContainer : VBoxContainer
{
    PackedScene game_scene;

    public GameContainer()
    {
        this.game_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/GameButton.tscn");
    }

    public void clearWorlds()
    {
        foreach (var c in this.GetChildren())
        {
            ((Node)c).QueueFree();
        }
    }

    public void addWorld(KnyttWorld<string> world, Texture icon)
    {
        var game_node = this.game_scene.Instance() as GameButton;
        game_node.initialize(icon, world);
        game_node.Connect("GamePressed", GetNode<LevelSelection>("../../.."), "_on_GamePressed");
        AddChild(game_node);
    }
}
