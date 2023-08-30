using Godot;

public class InputScreen : BasicScreen
{
    InputOption collecting;
    int cnum;

    public override void _Ready()
    {
        base._Ready();
        connectSettings(GetNode<Node>("SettingsContainer"));
        connectSettings(GetNode<Node>("KSSettingsContainer"));
        GetNode<HSlider>("Sensitivity/Slider").Value = GDKnyttSettings.StickSensitivity;
        GetNode<Control>("Sensitivity").Visible = GDKnyttKeys.HasAxis;
        initFocus();
    }

    public override void initFocus()
    {
        GetNode<Control>("SettingsContainer/UpSetting/Button0").GrabFocus();
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

    public override void goBack()
    {
        GDKnyttKeys.saveSettings();
        base.goBack();
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
        GetNode<Button>("BackButton").Disabled = true;
        GetNode<Button>("BackButton").FocusMode = Control.FocusModeEnum.None;
        GetNode<Control>("Sensitivity").Visible = false;
    }

    private void finishCollecting()
    {
        GetNode<Timer>("BounceTimer").Start();
        GDKnyttKeys.applyAllSettings();
        collecting.refreshButtons();
        collecting = null;
        GetNode<Control>("KeyPrompt").Visible = false;
        GetNode<Button>("BackButton").Disabled = false;
        GetNode<Button>("BackButton").FocusMode = Control.FocusModeEnum.All;
        GetNode<Control>("Sensitivity").Visible = GDKnyttKeys.HasAxis;
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

    private void _on_ToBack_focus_entered()
    {
        GetNode<Control>(GetNode<Control>("Sensitivity").Visible ? "Sensitivity/Slider" : "BackButton").GrabFocus();
    }

    private void _on_FromBack_focus_entered()
    {
        GetNode<Control>(GetNode<Control>("Sensitivity").Visible ? "Sensitivity/Slider" : "KSSettingsContainer/DieSetting/Button0").GrabFocus();
    }

    private void _on_FromBackLeft_focus_entered()
    {
        GetNode<Button>("SettingsContainer/WalkSetting/Button1").GrabFocus();
    }

    private void _on_FromSlider_focus_entered()
    {
        GetNode<Control>("KSSettingsContainer/DieSetting/Button0").GrabFocus();
    }

    private void _on_Slider_value_changed(float value)
    {
        GDKnyttSettings.StickSensitivity = value;
    }
}
