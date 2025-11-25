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
        //GetNode<Button>("TabsContainer/TouchTab").Disabled = GDKnyttSettings.FullHeightScroll;
        GetNode<Button>("TabsContainer/TouchTab").Visible = OS.GetName() != "Unix";
        GetNode<Button>("TabsContainer/DirTab").Visible = OS.GetName() != "iOS" && OS.GetName() != "HTML5";
        GetNode<Button>("GeneralContainer/Misc/DownloadSaves").Visible = OS.GetName() == "HTML5";
        if (OS.GetName() == "Unix")
        {
            GetNode<OptionButton>("GeneralContainer/Misc/ShaderContainer/Shader")
                .RemoveItem((int)GDKnyttSettings.ShaderType.HQ4X);
        }
    }

    public override void initFocus()
    {
        GetNode<Control>("TabsContainer/GeneralTab").GrabFocus();
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

    /*public void _on_ScollDropdown_item_selected(int index)
    {
        GDKnyttSettings.ScrollType = (GDKnyttSettings.ScrollTypes)index;

        if (GDKnyttSettings.SeamlessScroll)
        {
            GetNode<CheckBox>("SettingsContainer/Border").Pressed = true;
            GDKnyttSettings.Border = true;
        }

        GetNode<Button>("ButtonContainer/TouchPanelButton").Disabled = GDKnyttSettings.FullHeightScroll;
    }*/

    private void _on_ForcedMap_pressed()
    {
        GDKnyttSettings.ForcedMap = GetNode<CheckBox>("GeneralContainer/Graphics2/MapContainer/ForcedMap").Pressed;
    }

    private void _on_DetailedMap_pressed()
    {
        GDKnyttSettings.DetailedMap = GetNode<CheckBox>("GeneralContainer/Graphics2/MapContainer/DetailedMap").Pressed;
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
            tab_button.Name == "KeysTab"    ? "KeysContainer/StickContainter/SensitivitySlider" :
            tab_button.Name == "TouchTab"   ? "TouchContainer/SwipeContainer/SwipeDefaultButton" :
            tab_button.Name == "DirTab"     ? "DirContainer/SavesContainer/BackupButton" :
            tab_button.Name == "AudioTab"   ? "AudioContainer/EffectsPanningSlider" : "BackButton";
        GetNode<Control>(control_path).GrabFocus();
    }
}
