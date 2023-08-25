using Godot;

public class PauseLayer : BasicScreen
{
    public override void initFocus()
    {
        GetNode<Button>("PausePanel/ButtonContainer/ResumeButton").GrabFocus();
    }

    protected override void backEvent()
    {
        GetNode<PausePanel>("PausePanel").backEvent();
    }

    public override void goBack()
    {
        GetNode<PausePanel>("PausePanel").unpause();
    }
}
