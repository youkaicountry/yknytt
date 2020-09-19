using Godot;

public class SettingsScreen : CanvasLayer
{
    PackedScene input_scene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.input_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/InputScreen.tscn");
        fillControls();
    }

    public void fillControls()
    {
        GetNode<CheckBox>("SettingsContainer/FullScreen").Pressed = GDKnyttSettings.Fullscreen;
        GetNode<CheckBox>("SettingsContainer/SmoothScale").Pressed = GDKnyttSettings.SmoothScaling;
        GetNode<OptionButton>("SettingsContainer/ScrollContainer/ScrollDropdown").Select((int)GDKnyttSettings.ScrollType);
    }

    public void _on_BackButton_pressed()
    {
        GDKnyttSettings.saveSettings();
        ClickPlayer.Play();
        this.QueueFree();
    }

    public void _on_FullScreen_pressed()
    {
        GDKnyttSettings.Fullscreen = GetNode<CheckBox>("SettingsContainer/FullScreen").Pressed;
    }

    public void _on_SmoothScale_pressed()
    {
        GDKnyttSettings.SmoothScaling = GetNode<CheckBox>("SettingsContainer/SmoothScale").Pressed;
    }

    public void _on_ScollDropdown_item_selected(int index)
    {
        GDKnyttSettings.ScrollType = (GDKnyttSettings.ScrollTypes)index;
    }

    public void _on_KeyRemapButton_pressed()
    {
        ClickPlayer.Play();
        var inp = input_scene.Instance();
        AddChild(inp);
    }
}
