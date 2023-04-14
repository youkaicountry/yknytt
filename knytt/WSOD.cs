using Godot;

public partial class WSOD : ColorRect
{
    [Signal] public delegate void WSODFinishedEventHandler();

    public void startWSOD()
    {
        Visible = true;
        GetNode<Timer>("Timer").Start();
    }

    public async void _on_Timer_timeout()
    {
        EmitSignal(SignalName.WSODFinished);
        await ToSignal(GetTree().CreateTimer(.03f), "timeout");
        Visible = false;
    }
}
