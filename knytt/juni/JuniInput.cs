using Godot;
using System.Collections.Generic;

public partial class JuniInput
{
    class PressEdge
    {
        public PressEdge(string action) { this.action = action; }
        public string action;
        public bool justPressed;
        public bool justReleased;
        public bool lastState;

        public void Update()
        {
            var state = Input.IsActionPressed(action);
            justPressed = !lastState && state;
            justReleased = lastState && !state;
            lastState = state;
        }
    }

    private Dictionary<string, PressEdge> pressEdges;

    public bool Enabled { get; set; } = true;

    public JuniInput(Juni juni)
    {
        Juni = juni;

        // Register actions for JustPressed monitoring
        pressEdges = new Dictionary<string, PressEdge>()
        {
            ["down"] = new PressEdge("down"),
            ["umbrella"] = new PressEdge("umbrella"),
            ["hologram"] = new PressEdge("hologram"),
            ["jump"] = new PressEdge("jump")
        };
    }

    public Juni Juni { get; }
    public AltInput altInput = new AltInput();
    public bool checkPressed(string action)
    {
        return Enabled && ((!Juni.GDArea.BlockInput && Input.IsActionPressed(action)) || (Juni.GDArea.HasAltInput && altInput.IsActionPressed(action)));
    }
    public bool checkJustPressed(string action)
    {
        return Enabled && ((!Juni.GDArea.BlockInput && pressEdges[action].justPressed) || (Juni.GDArea.HasAltInput && altInput.IsActionJustPressed(action)));
    }
    public bool checkJustReleased(string action)
    {
        return (!Juni.GDArea.BlockInput && pressEdges[action].justReleased) || (Juni.GDArea.HasAltInput && altInput.IsActionJustReleased(action));
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

    public void Update()
    {
        pressEdges["down"].Update();
        pressEdges["umbrella"].Update();
        pressEdges["hologram"].Update();
        pressEdges["jump"].Update();
    }

    public void FinishFrame()
    {
        altInput.FinishFrame();
    }
}
