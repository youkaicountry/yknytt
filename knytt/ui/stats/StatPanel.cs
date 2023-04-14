using Godot;

public partial class StatPanel : Panel
{
    private static readonly string[] powers = {"Run", "Climb", "Double Jump", "High Jump", "Eye", "Radar", "Umbrella", "Hologram",
        "Red Key", "Yellow Key", "Blue Key", "Purple Key", "Map"};

    private PackedScene itemScene;
    private PackedScene labelScene;
    private StyleBox achievedStyle;
    private Control container;

    public override void _Ready()
    {
        itemScene = ResourceLoader.Load<PackedScene>("res://knytt/ui/stats/StatItem.tscn");
        labelScene = ResourceLoader.Load<PackedScene>("res://knytt/ui/stats/StatLabel.tscn");
        achievedStyle = ResourceLoader.Load<StyleBox>("res://knytt/ui/styles/AchievedItem.tres");
        container = GetNode<Control>("ScrollContainer/VBoxContainer");
    }

    public void addLabel(string text)
    {
        GetNodeOrNull<AnimatedSprite2D>("ScrollContainer/VBoxContainer/AnimatedSprite2D")?.QueueFree();
        var label = labelScene.Instantiate<Label>();
        label.Text = text;
        container.AddChild(label);
    }

    public void addPower(int i, int count, bool achieved)
    {
        addItem($"res://knytt/ui/stats/Power{i}.tres", powers[i], count, achieved);
    }

    public void addCutscene(string name, int count, bool achieved)
    {
        addItem($"res://knytt/ui/stats/Cutscene.tres", name, count, achieved);
    }

    public void addEnding(string name, int count, bool achieved)
    {
        addItem($"res://knytt/ui/stats/Ending.tres", name, count, achieved);
    }

    private void addItem(string texture, string label, int count, bool achieved)
    {
        var node = itemScene.Instantiate<Control>();
        node.GetNode<TextureRect>("HBoxContainer/TextureRect").Texture = ResourceLoader.Load<Texture2D>(texture);
        node.GetNode<Label>("HBoxContainer/VBoxContainer/ItemLabel").Text = label;
        node.GetNode<Label>("HBoxContainer/VBoxContainer/CountLabel").Text = $"Achieved {count} times";
        if (achieved) { node.AddThemeStyleboxOverride("panel", achievedStyle); }
        container.AddChild(node);
    }
}
