using Godot;

public class StatPanel : Panel
{
    private static readonly string[] powers = {"Run", "Climb", "Double Jump", "High Jump", "Eye", "Radar", "Umbrella", "Hologram", 
        "Red Key", "Yellow Key", "Blue Key", "Purple Key", "Map"};

    private PackedScene itemScene;
    private PackedScene labelScene;
    private Control container;

    public override void _Ready()
    {
        itemScene = ResourceLoader.Load<PackedScene>("res://knytt/ui/stats/StatItem.tscn");
        labelScene = ResourceLoader.Load<PackedScene>("res://knytt/ui/stats/StatLabel.tscn");
        container = GetNode<Control>("ScrollContainer/VBoxContainer");
    }

    public void addLabel(string text)
    {
        var label = labelScene.Instance() as Label;
        label.Text = text;
        container.AddChild(label);
    }

    public void addPower(int i, int count)
    {
        addItem($"res://knytt/ui/stats/Power{i}.tres", powers[i], count);
    }

    public void addCutscene(string name, int count)
    {
        addItem($"res://knytt/ui/stats/Cutscene.tres", name, count);
    }

    public void addEnding(string name, int count)
    {
        addItem($"res://knytt/ui/stats/Ending.tres", name, count);
    }

    private void addItem(string texture, string label, int count)
    {
        var node = itemScene.Instance();
        node.GetNode<TextureRect>("TextureRect").Texture = ResourceLoader.Load<Texture>(texture);
        node.GetNode<Label>("VBoxContainer/ItemLabel").Text = label;
        node.GetNode<Label>("VBoxContainer/CountLabel").Text = $"Achieved {count} times";
        container.AddChild(node);
    }
}
