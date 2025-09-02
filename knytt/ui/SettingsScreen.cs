using System;
using System.IO.Compression;
using Godot;
using YKnyttLib.Logging;

public class SettingsScreen : BasicScreen
{
    PackedScene input_scene;
    PackedScene touch_scene;
    PackedScene dir_scene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        this.input_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/InputScreen.tscn");
        this.touch_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/touch/TouchSettingsScreen.tscn");
        this.dir_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/DirectoriesScreen.tscn");
        GDKnyttSettings.setupViewport(for_ui: true, check_shrink: true);
        fillControls();
        initFocus();
    }

    public void fillControls()
    {
        GetNode<CheckBox>("SettingsContainer/FullScreen").Pressed = GDKnyttSettings.Fullscreen;
        GetNode<CheckBox>("SettingsContainer/SmoothScale").Pressed = GDKnyttSettings.SmoothScaling;
        GetNode<OptionButton>("SettingsContainer/ScrollDropdown").Select((int)GDKnyttSettings.ScrollType);
        GetNode<CheckBox>("SettingsContainer/Border").Pressed = GDKnyttSettings.Border;
        GetNode<CheckBox>("SettingsContainer/WSOD").Pressed = GDKnyttSettings.WSOD;
        GetNode<CheckBox>("SettingsContainer/MapContainer/ForcedMap").Pressed = GDKnyttSettings.ForcedMap;
        GetNode<CheckBox>("SettingsContainer/MapContainer/DetailedMap").Pressed = GDKnyttSettings.DetailedMap;
        GetNode<OptionButton>("ButtonContainer/OfflineDropdown").Select((int)GDKnyttSettings.Connection);
        GetNode<OptionButton>("ButtonContainer/ShaderContainer/Shader").Select((int)GDKnyttSettings.Shader);
        GetNode<Slider>("VolumeContainer/MasterVolumeSlider").Value = GDKnyttSettings.MasterVolume;
        GetNode<Slider>("VolumeContainer/MusicVolumeSlider").Value = GDKnyttSettings.MusicVolume;
        GetNode<Slider>("VolumeContainer/EnvironmentVolumeSlider").Value = GDKnyttSettings.EnvironmentVolume;
        GetNode<Slider>("VolumeContainer/EffectsVolumeSlider").Value = GDKnyttSettings.EffectsVolume;
        GetNode<Slider>("VolumeContainer/EffectsPanningSlider").Value = GDKnyttSettings.EffectsPanning;
        GetNode<CheckBox>("SettingsContainer/FullScreen").Visible = !GDKnyttSettings.Mobile && OS.GetName() != "HTML5";
        GetNode<Button>("ButtonContainer/DirButton").Visible = OS.GetName() != "iOS";
        if (OS.GetName() == "HTML5") { GetNode<Button>("ButtonContainer/DirButton").Text = "Download Saves"; }
        GetNode<Button>("ButtonContainer/TouchPanelButton").Disabled = GDKnyttSettings.FullHeightScroll;
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
        GDKnyttSettings.setupViewport(for_ui: true, check_shrink: false);
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

        if (GDKnyttSettings.SeamlessScroll)
        {
            GetNode<CheckBox>("SettingsContainer/Border").Pressed = true;
            GDKnyttSettings.Border = true;
        }

        GetNode<Button>("ButtonContainer/TouchPanelButton").Disabled = GDKnyttSettings.FullHeightScroll;
    }

    private void _on_Border_pressed()
    {
        GDKnyttSettings.Border = GetNode<CheckBox>("SettingsContainer/Border").Pressed;
    }

    private void _on_ForcedMap_pressed()
    {
        GDKnyttSettings.ForcedMap = GetNode<CheckBox>("SettingsContainer/MapContainer/ForcedMap").Pressed;
    }

    private void _on_DetailedMap_pressed()
    {
        GDKnyttSettings.DetailedMap = GetNode<CheckBox>("SettingsContainer/MapContainer/DetailedMap").Pressed;
    }
    
    private void _on_Shader_item_selected(int index)
    {
        GDKnyttSettings.Shader = (GDKnyttSettings.ShaderType)index;
    }

    public void _on_KeyRemapButton_pressed()
    {
        loadScreen(input_scene.Instance() as BasicScreen);
    }

    private void _on_TouchPanelButton_pressed()
    {
        loadScreen(touch_scene.Instance() as BasicScreen);
    }

    private void _on_DirButton_pressed()
    {
        if (OS.GetName() == "HTML5")
        {
            string dest_zip = GDKnyttDataStore.BaseDataDirectory.PlusFile("yknytt-saves.zip");
            if (System.IO.File.Exists(dest_zip)) { System.IO.File.Delete(dest_zip); }

            try
            {
                ZipFile.CreateFromDirectory(GDKnyttDataStore.BaseDataDirectory.PlusFile("Saves"), dest_zip);
            }
            catch (Exception e)
            {
                KnyttLogger.Error(e.Message);
            }
            JavaScript.DownloadBuffer(GDKnyttAssetManager.loadFile(dest_zip), "yknytt-saves.zip");
            return;
        }

        loadScreen(dir_scene.Instance() as BasicScreen);
    }

    private void _on_OfflineDropdown_item_selected(int index)
    {
        GDKnyttSettings.Connection = (GDKnyttSettings.ConnectionType)index;
    }

    private void _on_WSOD_pressed()
    {
        GDKnyttSettings.WSOD = GetNode<CheckBox>("SettingsContainer/WSOD").Pressed;
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

    private void _on_EffectsPanningSlider_value_changed(float value)
    {
        GDKnyttSettings.EffectsPanning = value;
    }

    private void _on_EnvironmentVolumeSlider_value_changed(float value)
    {
        GDKnyttSettings.EnvironmentVolume = (int)value;
    }
}
