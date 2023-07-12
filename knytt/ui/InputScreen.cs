using Godot;

public class InputScreen : BasicScreen
{
    InputOption collecting;
    int cnum;

    public override void _Ready()
    {
        connectSettings(GetNode<Node>("SettingsContainer"));
        connectSettings(GetNode<Node>("KSSettingsContainer"));
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
        if (collecting == null) { return; }
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
        GetNode<Button>("BackButton").GrabFocus();
    }

    private void _on_FromBack_focus_entered()
    {
        GetNode<Button>("KSSettingsContainer/DieSetting/Button0").GrabFocus();
    }

    private void _on_FromBackLeft_focus_entered()
    {
        GetNode<Button>("SettingsContainer/WalkSetting/Button1").GrabFocus();
    }
}
