using System;
using System.IO.Compression;
using System.Linq;
using Godot;
using YKnyttLib.Logging;

public class SettingsScreen : BasicScreen
{
    public override void _Ready()
    {
        base._Ready();
        GDKnyttSettings.setupViewport(for_ui: true, check_shrink: true);
        fillControls();
        initFocus();
    }

    public void fillControls()
    {
        GetNode<CheckBox>("GeneralContainer/Graphics/FullScreen").Pressed = GDKnyttSettings.Fullscreen;
        GetNode<CheckBox>("GeneralContainer/Graphics/SmoothScale").Pressed = GDKnyttSettings.SmoothScaling;
        GetNode<CheckBox>("GeneralContainer/Graphics/WSOD").Pressed = GDKnyttSettings.WSOD;
        GetNode<CheckBox>("GeneralContainer/Graphics/DownButtonHint").Pressed = GDKnyttSettings.DownButtonHint;
        GetNode<CheckBox>("GeneralContainer/Graphics/UmbrellaCheat").Pressed = GDKnyttSettings.UmbrellaCheat;
        setAspectClamped(GDKnyttSettings.Aspect);
        GetNode<CheckBox>("GeneralContainer/Graphics2/Seamless").Pressed = GDKnyttSettings.SeamlessScroll;
        GetNode<CheckBox>("GeneralContainer/Graphics2/Border").Pressed = GDKnyttSettings.Border;
        GetNode<CheckBox>("GeneralContainer/Graphics2/MapContainer/ForcedMap").Pressed = GDKnyttSettings.ForcedMap;
        GetNode<CheckBox>("GeneralContainer/Graphics2/MapContainer/DetailedMap").Pressed = GDKnyttSettings.DetailedMap;
        GetNode<OptionButton>("GeneralContainer/Misc/ShaderContainer/Shader").Select((int)GDKnyttSettings.Shader);
        GetNode<OptionButton>("GeneralContainer/Misc/OfflineDropdown").Select((int)GDKnyttSettings.Connection);
        
        GetNode<Slider>("AudioContainer/MasterVolumeSlider").Value = GDKnyttSettings.MasterVolume;
        GetNode<Slider>("AudioContainer/MusicVolumeSlider").Value = GDKnyttSettings.MusicVolume;
        GetNode<Slider>("AudioContainer/EnvironmentVolumeSlider").Value = GDKnyttSettings.EnvironmentVolume;
        GetNode<Slider>("AudioContainer/EffectsVolumeSlider").Value = GDKnyttSettings.EffectsVolume;
        GetNode<Slider>("AudioContainer/EffectsPanningSlider").Value = GDKnyttSettings.EffectsPanning;

        GetNode<CheckBox>("GeneralContainer/Graphics/FullScreen").Disabled = 
            GDKnyttSettings.Mobile || OS.GetName() == "HTML5" || OS.GetName() == "Unix";
        GetNode<Button>("TabsContainer/TouchTab").Visible = OS.GetName() != "Unix";
        GetNode<Button>("TabsContainer/DirTab").Visible = OS.GetName() != "iOS" && OS.GetName() != "HTML5";
        GetNode<Button>("GeneralContainer/Misc/DownloadSaves").Visible = OS.GetName() == "HTML5";
        GetNode<Label>("KeysContainer/SettingsContainer/LabelExit").Visible = GDKnyttDataStore.GptokeybMode;
        GetNode<Label>("GeneralContainer/Misc/LabelDetailed").Visible = GDKnyttSettings.DetailedMap;
        GetNode<Label>("GeneralContainer/Misc/LabelEmpty").RectMinSize = new Vector2(0, OS.GetName() == "HTML5" ? 17 : 52);
        if (OS.GetName() == "Unix" || OS.GetName() == "HTML5")
        {
            GetNode<OptionButton>("GeneralContainer/Misc/ShaderContainer/Shader")
                .RemoveItem((int)GDKnyttSettings.ShaderType.HQ4X);
        }
    }

    public override void initFocus()
    {
        GetNode<Control>("TabsContainer/GeneralTab").GrabFocus();
    }

    /* Use it after theme is adjusted
    +hint_tooltip = "Toggle between fullscreen and windowed mode"
    +hint_tooltip = "Use smooth high resolution scaling instead of pixelated (nearest neighbor)"
    +hint_tooltip = "Show a white screen when respawning instead of instant teleport"
    +hint_tooltip = "Show an arrow under Juni on switches and jump platforms"
    +hint_tooltip = "Fold the umbrella for higher jumps"
    +hint_tooltip = "Use integer resolution scales, or take 100% width or height"
    +hint_tooltip = "Amount of visible area"
    +hint_tooltip = "Smoothly scroll the camera between areas"
    +hint_tooltip = "Show a border around the game area"
    +hint_tooltip = "Show the map in levels that don't have it, but don't hide it intentionally"
    +hint_tooltip = "Show the map as an image"
    +hint_tooltip = "Apply a visual filter to the game"
    +hint_tooltip = "Download all save files in a zip"
    +hint_tooltip = "Control online features and achievement tracking"
    +hint_tooltip = "Overall game volume"
    +hint_tooltip = "Background music volume"
    +hint_tooltip = "Ambient sound effects volume"
    +hint_tooltip = "Sound effects volume"
    +hint_tooltip = "How much sound effects pan left/right based on position"
    */

    public override void goBack()
    {
        GDKnyttSettings.saveSettings();
        TouchSettings.applyAllSettings(GetTree());
        GDKnyttSettings.setupViewport(for_ui: true, check_shrink: false);
        base.goBack();
    }

    public void _on_FullScreen_pressed()
    {
        GDKnyttSettings.Fullscreen = GetNode<CheckBox>("GeneralContainer/Graphics/FullScreen").Pressed;
    }

    public void _on_SmoothScale_pressed()
    {
        GDKnyttSettings.SmoothScaling = GetNode<CheckBox>("GeneralContainer/Graphics/SmoothScale").Pressed;
    }

    private void _on_Border_pressed()
    {
        GDKnyttSettings.Border = GetNode<CheckBox>("GeneralContainer/Graphics2/Border").Pressed;
    }

    private void _on_WSOD_pressed()
    {
        GDKnyttSettings.WSOD = GetNode<CheckBox>("GeneralContainer/Graphics/WSOD").Pressed;
    }

    private void _on_DownButtonHint_pressed()
    {
        GDKnyttSettings.DownButtonHint = GetNode<CheckBox>("GeneralContainer/Graphics/DownButtonHint").Pressed;
    }

    private void _on_UmbrellaCheat_pressed()
    {
        GDKnyttSettings.UmbrellaCheat = GetNode<CheckBox>("GeneralContainer/Graphics/UmbrellaCheat").Pressed;
    }

    private void setAspectClamped(float value)
    {
        float window_x_fixed = OS.WindowSize.x * TouchSettings.ViewportNow;
        if (window_x_fixed < 600) { window_x_fixed = 600; }
        Slider slider = GetNode<Slider>("GeneralContainer/Graphics2/Aspect");
        slider.MinValue = Mathf.Floor(window_x_fixed / 600) * 240 / window_x_fixed;
        slider.MaxValue = 1 / (OS.WindowSize.Aspect() * TouchSettings.ViewportNow);
        slider.Value = value;
    }

    public void _on_Aspect_value_changed(float value)
    {
        setAspectClamped(value); // outsides are possible, because method is public
        value = (float)GetNode<Slider>("GeneralContainer/Graphics2/Aspect").Value;
        GDKnyttSettings.Aspect = value;
        GDKnyttSettings.setupCamera();

        GetNode<Label>("GeneralContainer/Graphics2/Height/Value").Text = 
            $"Used Height: {OS.WindowSize.Aspect() * value * 100:F0}%";
        float window_x_fixed = OS.WindowSize.x * TouchSettings.ViewportNow;
        float scale = window_x_fixed * value / 240;
        if (!GDKnyttSettings.SideScroll) { window_x_fixed *= GDKnyttSettings.Aspect / 0.4f; }
        GetNode<Label>("GeneralContainer/Graphics2/Width/Resolution").Text = 
            $"{window_x_fixed:F0}x{240 * scale:F0} ({scale:F2}x)";
        GetNode<Label>("GeneralContainer/Graphics2/Width/Value").Text = $"Width: {100 * 0.4f / value:F0}%";
    }

    private void _on_IntScale_pressed()
    {
        float window_x_fixed = OS.WindowSize.x * TouchSettings.ViewportNow;
        var aspects = Enumerable.Range(1, 20).Select(i => i * 240 / window_x_fixed).ToList(); // integer scales
        aspects.Add(1 / (OS.WindowSize.Aspect() * TouchSettings.ViewportNow)); // height = max
        aspects.Add(1 / OS.WindowSize.Aspect()); // height = 100%
        aspects.Add(0.4f);                       // width = 100%
        var cur_value = (float)GetNode<Slider>("GeneralContainer/Graphics2/Aspect").Value;
        GetNode<Slider>("GeneralContainer/Graphics2/Aspect").Value =
            aspects.OrderBy(a => Mathf.Abs(a - cur_value)).First();
    }

    private void _on_Seamless_pressed()
    {
        GDKnyttSettings.SeamlessScroll = GetNode<CheckBox>("GeneralContainer/Graphics2/Seamless").Pressed;
        GDKnyttSettings.setupCamera();
        if (GDKnyttSettings.SeamlessScroll)
        {
            GetNode<CheckBox>("GeneralContainer/Graphics2/Border").Pressed = true;
            GDKnyttSettings.Border = true;
        }
    }

    private void _on_ForcedMap_pressed()
    {
        GDKnyttSettings.ForcedMap = GetNode<CheckBox>("GeneralContainer/Graphics2/MapContainer/ForcedMap").Pressed;
    }

    private void _on_DetailedMap_pressed()
    {
        var detailed = GetNode<CheckBox>("GeneralContainer/Graphics2/MapContainer/DetailedMap").Pressed;
        GetNode<Label>("GeneralContainer/Misc/LabelDetailed").Visible = detailed;
        GDKnyttSettings.DetailedMap = detailed;
    }
    
    private void _on_Shader_item_selected(int index)
    {
        GDKnyttSettings.Shader = (GDKnyttSettings.ShaderType)index;
    }

    private void _on_DownloadSaves_pressed()
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
    }

    private void _on_OfflineDropdown_item_selected(int index)
    {
        GDKnyttSettings.Connection = (GDKnyttSettings.ConnectionType)index;
    }

    private void updateVolumeLabel(string item, int value)
    {
        GetNode<Label>($"AudioContainer/{item}VolumeLabel/Value").Text = $"{value}%";
    }

    private void _on_MasterVolumeSlider_value_changed(float value)
    {
        GDKnyttSettings.MasterVolume = (int)value;
        updateVolumeLabel("Master", (int)value);
    }

    private void _on_MusicVolumeSlider_value_changed(float value)
    {
        GDKnyttSettings.MusicVolume = (int)value;
        updateVolumeLabel("Music", (int)value);
    }

    private void _on_EnvironmentVolumeSlider_value_changed(float value)
    {
        GDKnyttSettings.EnvironmentVolume = (int)value;
        updateVolumeLabel("Environment", (int)value);
    }

    private void _on_EffectsVolumeSlider_value_changed(float value)
    {
        GDKnyttSettings.EffectsVolume = (int)value;
        updateVolumeLabel("Effects", (int)value);
    }

    private void _on_EffectsPanningSlider_value_changed(float value)
    {
        GDKnyttSettings.EffectsPanning = value;
        GetNode<Label>("AudioContainer/EffectsPanningLabel/Value").Text = $"{value:F2}x";
    }

    private void _on_Tab_focus_entered(string container_name)
    {
        foreach (Button tab_button in GetNode<Control>("TabsContainer").GetChildren())
        {
            string short_name = tab_button.Name.Left(tab_button.Name.Length - 3);
            if (short_name == container_name) { continue; }
            tab_button.Pressed = false;
            GetNode<Control>(short_name + "Container").Visible = false;
        }
        GetNode<Button>($"TabsContainer/{container_name}Tab").Pressed = true;
        GetNode<Control>(container_name + "Container").Visible = true;
    }

    private void _on_Tab_toggled(bool button_pressed, string container_name)
    {
        var tab_button = GetNode<Button>($"TabsContainer/{container_name}Tab");
        if (tab_button.HasFocus() && !button_pressed) { tab_button.Pressed = true; }
    }

    private Control getActiveTab()
    {
        return GetNode<Control>("TabsContainer").GetChildren().Cast<Button>().First(b => b.Pressed);
    }

    private void _on_ActiveTab_focus_entered()
    {
        getActiveTab().GrabFocus();
    }

    private void _on_BottomControl_focus_entered()
    {
        var tab_button = getActiveTab();
        string control_path = 
            tab_button.Name == "GeneralTab" ? "GeneralContainer/Misc/OfflineDropdown" :
            tab_button.Name == "KeysTab"    ? (GDKnyttDataStore.GptokeybMode ?
                                                "KeysContainer/StickContainter/LeftStick" :
                                                "KeysContainer/StickContainter/SensitivitySlider") :
            tab_button.Name == "TouchTab"   ? "TouchContainer/SwipeContainer/SwipeDefaultButton" :
            tab_button.Name == "DirTab"     ? "DirContainer/SavesContainer/BackupButton" :
            tab_button.Name == "AudioTab"   ? "AudioContainer/EffectsPanningSlider" : "BackButton";
        GetNode<Control>(control_path).GrabFocus();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (OS.GetName() == "Unix" && GetNode<Control>("BackButton").GetFocusOwner() is Slider slider)
        {
            if (Input.IsActionPressed("ui_left")) { slider.Value -= slider.Step; }
            if (Input.IsActionPressed("ui_right")) { slider.Value += slider.Step; }
        }
    }
}
