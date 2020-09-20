using Godot;

public class InputScreen : Control
{
    InputOption collecting;
    int cnum;

    public override void _Ready()
    {
        var settings = GetNode<Node>("SettingsContainer");
        foreach (var child in settings.GetChildren())
        {
            var io = child as InputOption;
            io.Screen = this;
            io.Connect("GetActionInput", this, "_onPress");
        }
    }

    public void _on_GDKnyttButton_pressed()
    {
        GDKnyttKeys.applyAllSettings();
        GDKnyttKeys.saveSettings();
        ClickPlayer.Play();
        QueueFree();
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
        GetNode<Button>("BackButton").FocusMode = FocusModeEnum.None;
    }

    private void finishCollecting()
    {
        GetNode<Timer>("BounceTimer").Start();
        collecting.refreshButtons();
        collecting = null;
        GetNode<Control>("KeyPrompt").Visible = false;
        GetNode<Button>("BackButton").Disabled = false;
        GetNode<Button>("BackButton").FocusMode = FocusModeEnum.All;
    }

    public void _on_CancelButton_pressed()
    {
        ClickPlayer.Play();
        finishCollecting();
    }
}
