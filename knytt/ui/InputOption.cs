using Godot;

public partial class InputOption : HBoxContainer
{
    [Export] public string Action;
    [Export] public string ActionLabel;

    [Signal] public delegate void GetActionInputEventHandler(InputOption io, int num);

    public InputScreen Screen { get; set; }

    public override void _Ready()
    {
        GetNode<Label>("Label").Text = ActionLabel;
        refreshButtons();
    }

    public void refreshButtons()
    {
        refreshButton(0);
        refreshButton(1);
    }

    public void setCollecting(int num)
    {
        GetNode<Button>("Button" + num).Text = "...";
    }

    private void refreshButton(int num)
    {
        GetNode<Button>("Button" + num).Text = GDKnyttKeys.getValueString(Action + num);
    }

    private void _on_Button0_pressed()
    {
        EmitSignal(SignalName.GetActionInput, this, 0);
    }

    private void _on_Button1_pressed()
    {
        EmitSignal(SignalName.GetActionInput, this, 1);
    }
}
