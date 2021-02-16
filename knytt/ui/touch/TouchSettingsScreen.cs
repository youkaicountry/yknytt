using Godot;

public class TouchSettingsScreen : Control
{
    public override void _Ready()
    {
        fillControls();
    }

    public void fillControls()
    {
        GetNode<CheckBox>("SettingsContainer/EnableButton").Pressed = TouchSettings.EnablePanel;
        GetNode<CheckBox>("SettingsContainer/SwapButton").Pressed = TouchSettings.SwapHands;
        GetNode<OptionButton>("SettingsContainer/AnchorContainer/AnchorDropdown").Select((int)TouchSettings.Position);
        GetNode<HSlider>("SettingsContainer/ScaleContainer2/ScaleSlider").Value = TouchSettings.Scale;
    }

    private void _on_EnableButton_pressed()
    {
        TouchSettings.EnablePanel = GetNode<CheckBox>("SettingsContainer/EnableButton").Pressed;
    }

    private void _on_SwapButton_pressed()
    {
        TouchSettings.SwapHands = GetNode<CheckBox>("SettingsContainer/SwapButton").Pressed;
    }

    private void _on_AnchorDropdown_item_selected(int index)
    {
        TouchSettings.Position = (TouchSettings.VerticalPosition)index;
    }

    private void _on_ScaleSlider_value_changed(float value)
    {
        TouchSettings.Scale = value;
        GetNode<Label>("SettingsContainer/ScaleContainer2/ValueLabel").Text = $"x{value:0.00}";
    }

    private void _on_ScaleDefaultButton_pressed()
    {
        GetNode<HSlider>("SettingsContainer/ScaleContainer2/ScaleSlider").Value = 1;
    }

    private void _on_BackButton_pressed()
    {
        ClickPlayer.Play();
        QueueFree();
    }
}
