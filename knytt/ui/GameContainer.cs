using Godot;

public class GameContainer : VBoxContainer
{
    PackedScene game_scene;

    public GameContainer()
    {
        this.game_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/GameButton.tscn");
    }

    public int Count { get { return GetChildCount(); } }

    public override void _Ready()
    {
        
    }

    public void clearWorlds()
    {
        foreach (var c in this.GetChildren())
        {
            ((Node)c).QueueFree();
        }
    }

    public void addWorld(GDKnyttWorldImpl world, Texture icon, bool focus)
    {
        var game_node = this.game_scene.Instance() as GameButton;
        game_node.initialize(icon, world);
        game_node.Connect("GamePressed", GetNode<LevelSelection>("../../.."), "_on_GamePressed");
        AddChild(game_node);
        if (focus) { game_node.GrabFocus(); }
    }
}
