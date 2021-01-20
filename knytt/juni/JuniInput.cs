using Godot;

public class JuniInput
{
    class PressEdge
    {
        public PressEdge(string action) { this.action = action; }
        public string action;
        public bool justPressed;
        public bool lastState;

        public void Update()
        {
            var state = Input.IsActionPressed(action);
            justPressed = !lastState && state;
            lastState = state;
        }
    }

    private PressEdge downPressed, umbrellaPressed, hologramPressed, jumpPressed;

    public JuniInput(Juni juni)
    {
        Juni = juni;

        // Register actions for JustPressed monitoring
        downPressed = new PressEdge("down");
        umbrellaPressed = new PressEdge("umbrella");
        hologramPressed = new PressEdge("hologram");
        jumpPressed = new PressEdge("jump");
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
    public bool DownPressed { get { return downPressed.justPressed; } }
    public bool UmbrellaPressed { get { return umbrellaPressed.justPressed; } }
    public bool HologramPressed { get { return hologramPressed.justPressed; } }
    public bool JumpEdge { get { return jumpPressed.justPressed; } }
    public bool JumpHeld { get { return checkPressed("jump"); } }
    public bool WalkHeld { get { return checkPressed("walk"); } }

    public void Update()
    {
        downPressed.Update();
        umbrellaPressed.Update();
        hologramPressed.Update();
        jumpPressed.Update();
    }

    public void FinishFrame()
    {
        altInput.FinishFrame();
    }
}
