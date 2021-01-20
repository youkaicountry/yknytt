using Godot;

public class JuniInput
{
    public JuniInput(Juni juni)
    {
        Juni = juni;
    }

    public Juni Juni { get; }
    public AltInput altInput = new AltInput();
    public bool checkPressed(string action)
    {
        return (!Juni.GDArea.BlockInput && Input.IsActionPressed(action)) || (Juni.GDArea.HasAltInput && altInput.IsActionPressed(action));
    }
    public bool checkJustPressed(string action)
    {
        return (!Juni.GDArea.BlockInput && Input.IsActionJustPressed(action)) || (Juni.GDArea.HasAltInput && altInput.IsActionJustPressed(action));
    }

    public bool LeftHeld { get { return checkPressed("left"); } }
    public bool RightHeld { get { return checkPressed("right"); } }
    public bool UpHeld { get { return checkPressed("up"); } }
    public bool DownHeld { get { return checkPressed("down"); } }
    public bool DownPressed { get { return checkJustPressed("down"); } }
    public bool UmbrellaPressed { get { return checkJustPressed("umbrella"); } }
    public bool HologramPressed { get { return checkJustPressed("hologram"); } }
    public bool JumpEdge { get { return checkJustPressed("jump"); } }
    public bool JumpHeld { get { return checkPressed("jump"); } }
    public bool WalkHeld { get { return checkPressed("walk"); } }

    public void FinishFrame()
    {
        altInput.FinishFrame();
    }
}

