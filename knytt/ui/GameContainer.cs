using Godot;

public class GameContainer : VBoxContainer
{
    PackedScene game_scene;

    HBoxContainer current_container = null;

    public GameContainer()
    {
        this.game_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/GameButton.tscn");
    }

    public int Count { get { return GetChildCount(); } }

    public void clearWorlds()
    {
        foreach (var c in this.GetChildren())
        {
            ((Node)c).QueueFree();
        }

        current_container = null;
    }

    public void addWorld(GDKnyttWorldImpl world, Texture icon, bool focus)
    {
        var game_node = this.game_scene.Instance() as GameButton;
        game_node.initialize(icon, world);
        game_node.Connect("GamePressed", GetNode<LevelSelection>("../../.."), "_on_GamePressed");
        
        if (current_container == null)
        {
            current_container = new HBoxContainer();
            AddChild(current_container);
            current_container.AddChild(game_node);
        }
        else
        {
            current_container.AddChild(game_node);
            current_container = null;
        }

        if (focus) { game_node.GrabFocus(); }
    }
}
