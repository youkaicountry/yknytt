using Godot;
using YKnyttLib;

public class GameContainer : VBoxContainer
{
    PackedScene game_scene;

    public GameContainer()
    {
        this.game_scene = ResourceLoader.Load("res://knytt/ui/GameButton.tscn") as PackedScene;
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
        AddChild(game_node);
    }
}
