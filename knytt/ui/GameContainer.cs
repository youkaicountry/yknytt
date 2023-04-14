using Godot;
using System.Collections.Generic;

public partial class GameContainer : VBoxContainer
{
    private PackedScene game_scene;

    private HBoxContainer current_container = null;

    private LinkedList<GameButton> stubs = new LinkedList<GameButton>();

    public int GamesCount { get; private set; }

    public const int BUTTON_HEIGHT = 59;

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
        stubs.Clear();
        GamesCount = 0;
    }

    public void addWorld(WorldEntry world_entry, bool focus = false, bool mark_completed = false)
    {
        bool replace_stub = stubs.Count != 0;
        var game_node = replace_stub ? stubs.First.Value : this.game_scene.Instantiate<GameButton>();
        if (replace_stub) { stubs.RemoveFirst(); }
        game_node.initialize(world_entry);
        game_node.GamePressed += GetNode<LevelSelection>("../../..")._on_GamePressed;
        if (!replace_stub) { addButton(game_node); }
        
        if (focus) { game_node.GrabFocus(); }
        if (mark_completed) { game_node.markCompleted(); }
        GamesCount++;
    }

    public void fillStubs(int count)
    {
        for (int i = GamesCount + stubs.Count; i < count; i++)
        {
            var game_node = this.game_scene.Instantiate<GameButton>();
            stubs.AddLast(game_node);
            addButton(game_node);
        }

        for (int i = GamesCount + stubs.Count; i > count; i--)
        {
            if (stubs.Count > 0)
            {
                if (i % 2 == 1) { stubs.Last.Value.GetParent().QueueFree(); }
                stubs.Last.Value.QueueFree();
                stubs.RemoveLast();
            }
        }

        current_container = count % 2 == 1 ? stubs.Last?.Value.GetParent<HBoxContainer>() : null;
    }

    private void addButton(GameButton game_node)
    {
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
    }
}
