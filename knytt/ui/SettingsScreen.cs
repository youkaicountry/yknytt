using Godot;

public class SettingsScreen : CanvasLayer
{
    PackedScene input_scene;
    PackedScene touch_scene;
    
    public bool CleanupViewport { set; get; } = false;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.input_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/InputScreen.tscn");
        this.touch_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/touch/TouchSettingsScreen.tscn");
        fillControls();
    }

    public void fillControls()
    {
        GetNode<CheckBox>("SettingsContainer/FullScreen").Pressed = GDKnyttSettings.Fullscreen;
        GetNode<CheckBox>("SettingsContainer/SmoothScale").Pressed = GDKnyttSettings.SmoothScaling;
        GetNode<OptionButton>("SettingsContainer/ScrollContainer/ScrollDropdown").Select((int)GDKnyttSettings.ScrollType);
        GetNode<Slider>("VolumeContainer/MasterVolumeSlider").Value = GDKnyttSettings.MasterVolume;
        GetNode<Slider>("VolumeContainer/MusicVolumeSlider").Value = GDKnyttSettings.MusicVolume;
        GetNode<Slider>("VolumeContainer/EffectsVolumeSlider").Value = GDKnyttSettings.EffectsVolume;
        GetNode<Slider>("VolumeContainer/EnvironmentVolumeSlider").Value = GDKnyttSettings.EnvironmentVolume;
    }

    public void _on_BackButton_pressed()
    {
        GDKnyttSettings.saveSettings();
        TouchSettings.applyAllSettings(GetTree());
        if (CleanupViewport) { GDKnyttSettings.setupViewport(for_ui: false); }
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

    private void _on_TouchPanelButton_pressed()
    {
        ClickPlayer.Play();
        var inp = touch_scene.Instance();
        AddChild(inp);
    }

    private void _on_MasterVolumeSlider_value_changed(float value)
    {
        GDKnyttSettings.MasterVolume = (int)value;
    }

    private void _on_MusicVolumeSlider_value_changed(float value)
    {
        GDKnyttSettings.MusicVolume = (int)value;
    }

    private void _on_EffectsVolumeSlider_value_changed(float value)
    {
        GDKnyttSettings.EffectsVolume = (int)value;
    }

    private void _on_EnvironmentVolumeSlider_value_changed(float value)
    {
        GDKnyttSettings.EnvironmentVolume = (int)value;
    }
}
