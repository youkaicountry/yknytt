using Godot;

public partial class TouchSettingsScreen : Control
{
    public override void _Ready()
    {
        fillControls();
    }

    public void fillControls()
    {
        GetNode<CheckBox>("SettingsContainer/EnableContainer/EnableButton").ButtonPressed = TouchSettings.EnablePanel;
        GetNode<CheckBox>("SettingsContainer/SwapContainer/SwapButton").ButtonPressed = TouchSettings.SwapHands;
        GetNode<OptionButton>("SettingsContainer/EnableContainer/AnchorDropdown").Select((int)TouchSettings.Position);
        GetNode<CheckBox>("SettingsContainer/SwipeContainer/SwipeButton").ButtonPressed = TouchSettings.Swipe;
        GetNode<HSlider>("SettingsContainer/ScaleContainer/ScaleSlider").Value = TouchSettings.Scale;
        GetNode<HSlider>("SettingsContainer/ViewportContainer/ViewportSlider").Value = TouchSettings.Viewport;
        GetNode<HSlider>("SettingsContainer/JumpContainer/JumpSlider").Value = TouchSettings.JumpScale;
        GetNode<HSlider>("SettingsContainer/OpacityContainer/OpacitySlider").Value = TouchSettings.Opacity;
    }

    private void _on_EnableButton_toggled(bool button_pressed)
    {
        TouchSettings.EnablePanel = button_pressed;
    }

    private void _on_SwapButton_toggled(bool button_pressed)
    {
        TouchSettings.SwapHands = button_pressed;
    }

    private void _on_AnchorDropdown_item_selected(int index)
    {
        TouchSettings.Position = (TouchSettings.VerticalPosition)index;
    }

    private void _on_SwipeButton_toggled(bool button_pressed)
    {
        TouchSettings.Swipe = button_pressed;
    }

    private void _on_ScaleSlider_value_changed(float value)
    {
        TouchSettings.Scale = value;
        GetNode<Label>("SettingsContainer/ScaleContainer/ValueLabel").Text = $"{(int)(value * 100)}%";
    }

    private void _on_ScaleDefaultButton_pressed()
    {
        GetNode<HSlider>("SettingsContainer/ScaleContainer/ScaleSlider").Value = 1;
    }

    private void _on_ViewportSlider_value_changed(float value)
    {
        TouchSettings.Viewport = value;
        GetNode<Label>("SettingsContainer/ViewportContainer/ValueLabel").Text = $"{(int)(value * 100)}%";
    }

    private void _on_JumpSlider_value_changed(float value)
    {
        TouchSettings.JumpScale = value;
        GetNode<Label>("SettingsContainer/JumpContainer/ValueLabel").Text = $"{(int)(value * 100)}%";
    }

    private void _on_JumpDefaultButton_pressed()
    {
        GetNode<HSlider>("SettingsContainer/JumpContainer/JumpSlider").Value = 1;
    }
    
    private void _on_OpacitySlider_value_changed(float value)
    {
        TouchSettings.Opacity = value;
        GetNode<Label>("SettingsContainer/OpacityContainer/ValueLabel").Text = $"{(int)(value * 100)}%";
    }

    private void _on_BackButton_pressed()
    {
        ClickPlayer.Play();
        QueueFree();
    }
}
