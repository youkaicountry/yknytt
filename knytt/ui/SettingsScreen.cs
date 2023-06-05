using Godot;

public class SettingsScreen : BasicScreeen
{
    PackedScene input_scene;
    PackedScene touch_scene;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.input_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/InputScreen.tscn");
        this.touch_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/touch/TouchSettingsScreen.tscn");
        fillControls();
        initFocus();
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
        GetNode<CheckBox>("SettingsContainer/FullScreen").Visible = OS.GetName() != "Android" && OS.GetName() != "iOS";
    }

    public override void initFocus()
    {
        bool desktop = GetNode<CheckBox>("SettingsContainer/FullScreen").Visible;
        GetNode<CheckBox>("SettingsContainer/" + (desktop ? "FullScreen": "SmoothScale")).GrabFocus();
    }

    public override void goBack()
    {
        GDKnyttSettings.saveSettings();
        TouchSettings.applyAllSettings(GetTree());
        base.goBack();
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
        loadScreen(input_scene.Instance() as BasicScreeen);
    }

    private void _on_TouchPanelButton_pressed()
    {
        loadScreen(touch_scene.Instance() as BasicScreeen);
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
