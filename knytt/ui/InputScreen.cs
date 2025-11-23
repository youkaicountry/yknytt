using Godot;

public class InputScreen : Control
{
    InputOption collecting;
    int cnum;

    public override void _Ready()
    {
        base._Ready();
        connectSettings(GetNode<Node>("SettingsContainer"));
        connectSettings(GetNode<Node>("KSSettingsContainer"));
        GetNode<HSlider>("StickContainter/SensitivitySlider").Value = GDKnyttSettings.StickSensitivity;
        GetNode<CheckBox>("StickContainter/LeftStick").Pressed = GDKnyttSettings.LeftStickMovement;
    }

    private void connectSettings(Node node)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is InputOption io)
            {
                io.Screen = this;
                io.Connect("GetActionInput", this, "_onPress");
            }
        }
    }

    public void _onPress(InputOption io, int num)
    {
        if (collecting != null || GetNode<Timer>("BounceTimer").TimeLeft > 0) { return; }
        ClickPlayer.Play();
        startCollecting(io, num);
    }

    public override void _Input(InputEvent @event)
    {
        if (collecting == null) { base._Input(@event); return; }
        if (@event.IsActionPressed("pause") || GDKnyttKeys.setAction(collecting.Action + cnum, @event))
        {
            finishCollecting();
        }
    }

    private void startCollecting(InputOption io, int num)
    {
        collecting = io;
        cnum = num;
        io.setCollecting(num);
        GetNode<Control>("KeyPrompt").Visible = true;
        GetNode<Button>("../BackButton").Visible = false;
        GetNode<Control>("StickContainter").Visible = false;
    }

    private void finishCollecting()
    {
        GetNode<Timer>("BounceTimer").Start();
        GDKnyttKeys.applyAllSettings();
        GDKnyttKeys.saveSettings();
        collecting.refreshButtons();
        collecting = null;
        GetNode<Control>("KeyPrompt").Visible = false;
        GetNode<Button>("../BackButton").Visible = true;
        GetNode<Control>("StickContainter").Visible = true;
    }

    public void _on_CancelButton_pressed()
    {
        ClickPlayer.Play();
        finishCollecting();
    }

    private void _on_ClearButton_pressed()
    {
        ClickPlayer.Play();
        GDKnyttKeys.setAction(collecting.Action + cnum, null);
        finishCollecting();
    }

    private void _on_ToTab_focus_entered()
    {
        GetNode<Button>("../TabsContainer/KeysTab").GrabFocus();
    }

    private void _on_FromTab_focus_entered()
    {
        GetNode<Button>("SettingsContainer/JumpSetting/Button0").GrabFocus();
    }

    private void _on_ToStick_focus_entered()
    {
        GetNode<Control>("StickContainter/LeftStick").GrabFocus();
    }

    private void _on_FromStickUp_focus_entered()
    {
        GetNode<Control>("KSSettingsContainer/DieSetting/Button0").GrabFocus();
    }

    private void _on_FromStickSide_focus_entered()
    {
        GetNode<Button>("SettingsContainer/MapSetting/Button0").GrabFocus();
    }

    private void _on_LeftStick_pressed()
    {
        GDKnyttSettings.LeftStickMovement = GetNode<CheckBox>("StickContainter/LeftStick").Pressed;
        GDKnyttKeys.applyAllSettings();
    }

    private void _on_Slider_value_changed(float value)
    {
        GDKnyttSettings.StickSensitivity = value;
    }
}
