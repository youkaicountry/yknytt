using Godot;

public class PauseLayer : BasicScreeen
{
    public override void initFocus()
    {
        GetNode<Button>("PausePanel/ButtonContainer/ResumeButton").GrabFocus();
    }

    protected override void backEvent()
    {
        GetNode<PausePanel>("PausePanel").backEvent();
    }
}