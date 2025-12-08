using Godot;

public class TouchSettingsScreen : Control
{
    public override void _Ready()
    {
        base._Ready();
        fillControls();
    }

    public void fillControls()
    {
        GetNode<CheckBox>("EnableContainer/EnableButton").Pressed = TouchSettings.EnablePanel;
        GetNode<CheckBox>("EnableContainer/SwapButton").Pressed = TouchSettings.SwapHands;
        GetNode<OptionButton>("EnableContainer/AnchorDropdown").Select((int)TouchSettings.Position);
        GetNode<HSlider>("SwipeContainer/SwipeSlider").Value = TouchSettings.Swipe;
        GetNode<HSlider>("ScaleContainer/ScaleSlider").Value = TouchSettings.Scale;
        GetNode<HSlider>("ViewportContainer/ViewportSlider").Value = TouchSettings.Viewport;
        GetNode<HSlider>("JumpContainer/JumpSlider").Value = TouchSettings.JumpScale;
        GetNode<HSlider>("OpacityContainer/OpacitySlider").Value = TouchSettings.Opacity;
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

    private void _on_SwipeSlider_value_changed(float value)
    {
        TouchSettings.Swipe = value;
        GetNode<Label>("SwipeContainer/ValueLabel").Text = $"{value * 100}%";
    }

    private void _on_SwipeDefaultButton_pressed()
    {
        GetNode<HSlider>("SwipeContainer/SwipeSlider").Value = 1;
    }

    private void _on_ScaleSlider_value_changed(float value)
    {
        TouchSettings.Scale = value;
        GetNode<Label>("ScaleContainer/ValueLabel").Text = $"{value * 100}%";
    }

    private void _on_ScaleDefaultButton_pressed()
    {
        GetNode<HSlider>("ScaleContainer/ScaleSlider").Value = 1;
    }

    private void _on_ViewportSlider_value_changed(float value)
    {
        TouchSettings.Viewport = value;
        GetNode<Label>("ViewportContainer/ValueLabel").Text = $"{value * 100}%";
        GetParent<SettingsScreen>()._on_Aspect_value_changed(GDKnyttSettings.Aspect);
    }

    private void _on_ViewportMaxButton_pressed()
    {
        GetNode<HSlider>("ViewportContainer/ViewportSlider").Value = 1;
    }

    private void _on_JumpSlider_value_changed(float value)
    {
        TouchSettings.JumpScale = value;
        GetNode<Label>("JumpContainer/ValueLabel").Text = $"{value * 100}%";
    }

    private void _on_JumpDefaultButton_pressed()
    {
        GetNode<HSlider>("JumpContainer/JumpSlider").Value = 1;
    }
    
    private void _on_OpacitySlider_value_changed(float value)
    {
        TouchSettings.Opacity = value;
        GetNode<Label>("OpacityContainer/ValueLabel").Text = $"{value * 100}%";
    }

    private void _on_OpacityMaxButton_pressed()
    {
        GetNode<HSlider>("OpacityContainer/OpacitySlider").Value = 1;
    }
}
