using Godot;

public class WSOD : ColorRect
{
    [Signal] public delegate void WSODFinished();

    public void startWSOD()
    {
        Visible = true;
        GetNode<Timer>("Timer").Start();
    }

    public async void _on_Timer_timeout()
    {
        EmitSignal("WSODFinished");
        await ToSignal(GetTree().CreateTimer(.03f), "timeout");
        Visible = false;
    }
}
