using Godot;

public class WSOD : ColorRect
{
    [Signal] public delegate void WSODFinished();

    public void startWSOD()
    {
        Visible = true;
        GetNode<Timer>("Timer").Start();
    }
    
    public void _on_Timer_timeout()
    {
        Visible = false;
        EmitSignal("WSODFinished");
    }
}
