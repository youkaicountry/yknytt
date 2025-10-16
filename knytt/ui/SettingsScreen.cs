using System;
using System.IO.Compression;
using Godot;
using YKnyttLib.Logging;

public class SettingsScreen : BasicScreen
{
    PackedScene input_scene;
    PackedScene touch_scene;
    PackedScene dir_scene;

    // Tooltip texts - easy to modify!
    private const string TOOLTIP_FULLSCREEN = "Toggle between fullscreen and windowed mode";
    private const string TOOLTIP_SMOOTH_SCALE = "Use smooth high resolution scaling instead of pixelated (nearest neighbor)";
    private const string TOOLTIP_BORDER = "Show a border around the game area";
    private const string TOOLTIP_SCROLL = "Choose how the camera scrolls between areas";
    private const string TOOLTIP_SHADER = "Apply a visual filter to the game";
    private const string TOOLTIP_WSOD = "Show a white screen when respawning instead of instant teleport";
    private const string TOOLTIP_MAP_FORCED = "Always show the map, even in levels that hide it";
    private const string TOOLTIP_MAP_DETAILED = "Show detailed information on the map";
    private const string TOOLTIP_JUMP_BUFFER = "Allow jump input slightly before landing (higher = more forgiving)";
    private const string TOOLTIP_VOLUME_MASTER = "Overall game volume";
    private const string TOOLTIP_VOLUME_MUSIC = "Background music volume";
    private const string TOOLTIP_VOLUME_ENVIRONMENT = "Ambient sound effects volume";
    private const string TOOLTIP_VOLUME_EFFECTS = "Sound effects volume";
    private const string TOOLTIP_EFFECTS_PANNING = "How much sound effects pan left/right based on position";
    private const string TOOLTIP_KEY_CONFIGURE = "Customize keyboard controls";
    private const string TOOLTIP_TOUCH_PANEL = "Configure touch controls for mobile";
    private const string TOOLTIP_DIRECTORIES = "Set custom directories for worlds and saves";
    private const string TOOLTIP_SERVER = "Control online features and achievement tracking";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        this.input_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/InputScreen.tscn");
        this.touch_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/touch/TouchSettingsScreen.tscn");
        this.dir_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/DirectoriesScreen.tscn");
        GDKnyttSettings.setupViewport(for_ui: true, check_shrink: true);
        setupTooltips();
        fillControls();
        initFocus();
    }

    private void setupTooltips()
    {
        // Graphics tooltips
        GetNode<CheckBox>("TabContainer/Graphics/FullScreen").HintTooltip = TOOLTIP_FULLSCREEN;
        GetNode<CheckBox>("TabContainer/Graphics/SmoothScale").HintTooltip = TOOLTIP_SMOOTH_SCALE;
        GetNode<CheckBox>("TabContainer/Graphics/Border").HintTooltip = TOOLTIP_BORDER;
        GetNode<OptionButton>("TabContainer/Graphics/HBoxContainer/ScrollDropdown").HintTooltip = TOOLTIP_SCROLL;
        GetNode<OptionButton>("TabContainer/Graphics/HBoxContainer2/Shader").HintTooltip = TOOLTIP_SHADER;

        // Gameplay tooltips
        GetNode<CheckBox>("TabContainer/Gameplay/WSOD").HintTooltip = TOOLTIP_WSOD;
        GetNode<CheckBox>("TabContainer/Gameplay/MapContainer/ForcedMap").HintTooltip = TOOLTIP_MAP_FORCED;
        GetNode<CheckBox>("TabContainer/Gameplay/MapContainer/DetailedMap").HintTooltip = TOOLTIP_MAP_DETAILED;
        GetNode<HSlider>("TabContainer/Gameplay/JumpBufferContainer/JumpBufferSlider").HintTooltip = TOOLTIP_JUMP_BUFFER;

        // Audio tooltips
        GetNode<HSlider>("TabContainer/Audio/MasterVolumeSlider").HintTooltip = TOOLTIP_VOLUME_MASTER;
        GetNode<HSlider>("TabContainer/Audio/MusicVolumeSlider").HintTooltip = TOOLTIP_VOLUME_MUSIC;
        GetNode<HSlider>("TabContainer/Audio/EnvironmentVolumeSlider").HintTooltip = TOOLTIP_VOLUME_ENVIRONMENT;
        GetNode<HSlider>("TabContainer/Audio/EffectsVolumeSlider").HintTooltip = TOOLTIP_VOLUME_EFFECTS;
        GetNode<HSlider>("TabContainer/Audio/EffectsPanningSlider").HintTooltip = TOOLTIP_EFFECTS_PANNING;

        // Controls tooltips
        GetNode<Button>("TabContainer/Controls/KeyRemapButton").HintTooltip = TOOLTIP_KEY_CONFIGURE;
        GetNode<Button>("TabContainer/Controls/TouchPanelButton").HintTooltip = TOOLTIP_TOUCH_PANEL;

        // Misc tooltips
        GetNode<Button>("TabContainer/Misc/DirButton").HintTooltip = TOOLTIP_DIRECTORIES;
        GetNode<OptionButton>("TabContainer/Misc/HBoxContainer/OfflineDropdown").HintTooltip = TOOLTIP_SERVER;
    }

    public void fillControls()
    {
        // Graphics tab
        GetNode<CheckBox>("TabContainer/Graphics/FullScreen").Pressed = GDKnyttSettings.Fullscreen;
        GetNode<CheckBox>("TabContainer/Graphics/SmoothScale").Pressed = GDKnyttSettings.SmoothScaling;
        GetNode<OptionButton>("TabContainer/Graphics/HBoxContainer/ScrollDropdown").Select((int)GDKnyttSettings.ScrollType);
        GetNode<CheckBox>("TabContainer/Graphics/Border").Pressed = GDKnyttSettings.Border;
        GetNode<OptionButton>("TabContainer/Graphics/HBoxContainer2/Shader").Select((int)GDKnyttSettings.Shader);

        // Gameplay tab
        GetNode<CheckBox>("TabContainer/Gameplay/WSOD").Pressed = GDKnyttSettings.WSOD;
        GetNode<CheckBox>("TabContainer/Gameplay/MapContainer/ForcedMap").Pressed = GDKnyttSettings.ForcedMap;
        GetNode<CheckBox>("TabContainer/Gameplay/MapContainer/DetailedMap").Pressed = GDKnyttSettings.DetailedMap;

        var jumpSlider = GetNode<HSlider>("TabContainer/Gameplay/JumpBufferContainer/JumpBufferSlider");
        jumpSlider.Value = GDKnyttSettings.JumpBufferTime;
        updateJumpBufferLabel((int)jumpSlider.Value);

        // Audio tab
        var masterSlider = GetNode<HSlider>("TabContainer/Audio/MasterVolumeSlider");
        masterSlider.Value = GDKnyttSettings.MasterVolume;
        updateVolumeLabel("TabContainer/Audio/MasterVolumeLabel", (int)masterSlider.Value, "Master Volume");

        var musicSlider = GetNode<HSlider>("TabContainer/Audio/MusicVolumeSlider");
        musicSlider.Value = GDKnyttSettings.MusicVolume;
        updateVolumeLabel("TabContainer/Audio/MusicVolumeLabel", (int)musicSlider.Value, "Music Volume");

        var envSlider = GetNode<HSlider>("TabContainer/Audio/EnvironmentVolumeSlider");
        envSlider.Value = GDKnyttSettings.EnvironmentVolume;
        updateVolumeLabel("TabContainer/Audio/EnvironmentVolumeLabel", (int)envSlider.Value, "Environment Volume");

        var effectsSlider = GetNode<HSlider>("TabContainer/Audio/EffectsVolumeSlider");
        effectsSlider.Value = GDKnyttSettings.EffectsVolume;
        updateVolumeLabel("TabContainer/Audio/EffectsVolumeLabel", (int)effectsSlider.Value, "Effects Volume");

        var panningSlider = GetNode<HSlider>("TabContainer/Audio/EffectsPanningSlider");
        panningSlider.Value = GDKnyttSettings.EffectsPanning;
        updatePanningLabel((float)panningSlider.Value);

        // Misc tab
        GetNode<OptionButton>("TabContainer/Misc/HBoxContainer/OfflineDropdown").Select((int)GDKnyttSettings.Connection);

        // Platform-specific visibility
        GetNode<CheckBox>("TabContainer/Graphics/FullScreen").Visible = !GDKnyttSettings.Mobile && OS.GetName() != "HTML5";
        GetNode<Button>("TabContainer/Misc/DirButton").Visible = OS.GetName() != "iOS";
        if (OS.GetName() == "HTML5") { GetNode<Button>("TabContainer/Misc/DirButton").Text = "Download Saves"; }
        GetNode<Button>("TabContainer/Controls/TouchPanelButton").Disabled = GDKnyttSettings.FullHeightScroll;
    }

    private void updateJumpBufferLabel(int value)
    {
        GetNode<Label>("TabContainer/Gameplay/JumpBufferContainer/JumpBufferLabel").Text = $"Jump Buffer Time: {value}ms";
    }

    private void updateVolumeLabel(string labelPath, int value, string baseName)
    {
        GetNode<Label>(labelPath).Text = $"{baseName}: {value}%";
    }

    private void updatePanningLabel(float value)
    {
        GetNode<Label>("TabContainer/Audio/EffectsPanningLabel").Text = $"Effects Panning: {value:F2}x";
    }

    public override void initFocus()
    {
        bool desktop = GetNode<CheckBox>("TabContainer/Graphics/FullScreen").Visible;
        GetNode<CheckBox>("TabContainer/Graphics/" + (desktop ? "FullScreen": "SmoothScale")).GrabFocus();
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
        GDKnyttSettings.Fullscreen = GetNode<CheckBox>("TabContainer/Graphics/FullScreen").Pressed;
    }

    public void _on_SmoothScale_pressed()
    {
        GDKnyttSettings.SmoothScaling = GetNode<CheckBox>("TabContainer/Graphics/SmoothScale").Pressed;
    }

    public void _on_ScollDropdown_item_selected(int index)
    {
        GDKnyttSettings.ScrollType = (GDKnyttSettings.ScrollTypes)index;

        if (GDKnyttSettings.SeamlessScroll)
        {
            GetNode<CheckBox>("TabContainer/Graphics/Border").Pressed = true;
            GDKnyttSettings.Border = true;
        }

        GetNode<Button>("TabContainer/Controls/TouchPanelButton").Disabled = GDKnyttSettings.FullHeightScroll;
    }

    private void _on_Border_pressed()
    {
        GDKnyttSettings.Border = GetNode<CheckBox>("TabContainer/Graphics/Border").Pressed;
    }

    private void _on_ForcedMap_pressed()
    {
        GDKnyttSettings.ForcedMap = GetNode<CheckBox>("TabContainer/Gameplay/MapContainer/ForcedMap").Pressed;
    }

    private void _on_DetailedMap_pressed()
    {
        GDKnyttSettings.DetailedMap = GetNode<CheckBox>("TabContainer/Gameplay/MapContainer/DetailedMap").Pressed;
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
        GDKnyttSettings.WSOD = GetNode<CheckBox>("TabContainer/Gameplay/WSOD").Pressed;
    }

    private void _on_MasterVolumeSlider_value_changed(float value)
    {
        GDKnyttSettings.MasterVolume = (int)value;
        updateVolumeLabel("TabContainer/Audio/MasterVolumeLabel", (int)value, "Master Volume");
    }

    private void _on_MusicVolumeSlider_value_changed(float value)
    {
        GDKnyttSettings.MusicVolume = (int)value;
        updateVolumeLabel("TabContainer/Audio/MusicVolumeLabel", (int)value, "Music Volume");
    }

    private void _on_EffectsVolumeSlider_value_changed(float value)
    {
        GDKnyttSettings.EffectsVolume = (int)value;
        updateVolumeLabel("TabContainer/Audio/EffectsVolumeLabel", (int)value, "Effects Volume");
    }

    private void _on_EffectsPanningSlider_value_changed(float value)
    {
        GDKnyttSettings.EffectsPanning = value;
        updatePanningLabel(value);
    }

    private void _on_EnvironmentVolumeSlider_value_changed(float value)
    {
        GDKnyttSettings.EnvironmentVolume = (int)value;
        updateVolumeLabel("TabContainer/Audio/EnvironmentVolumeLabel", (int)value, "Environment Volume");
    }

    private void _on_JumpBufferSlider_value_changed(float value)
    {
        GDKnyttSettings.JumpBufferTime = (int)value;
        updateJumpBufferLabel((int)value);
    }
}
