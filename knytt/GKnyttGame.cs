using Godot;
using YKnyttLib;

public class GKnyttGame : Node2D
{
    // TODO: This is per-player stuff, and should eventually be abstracted
    public GKnyttArea CurrentArea { get; private set; }
    public GKnyttWorld World { get; private set; }
    public Camera2D Camera { get; private set; }

    public override void _Ready()
    {
        this.Camera = GetNode("Camera2D") as Camera2D;
        this.World = GetNode("GKnyttWorld") as GKnyttWorld;

        this.loadDemo();
    }

    private void loadDemo()
    {
        var wd = new Directory();
        var e = wd.Open("./worlds/Nifflas - The Machine");
        this.World.loadWorld(wd);

        this.changeArea(new KnyttPoint(1001, 1000));
    }

    public void changeArea(KnyttPoint new_area)
    {
        // This should inform the world that it needs an area loaded
        this.CurrentArea = this.World.instantiateArea(new_area);

        this.Camera.GlobalPosition = this.CurrentArea.GlobalCenter;

        GD.Print(this.CurrentArea.GlobalPosition);
        GD.Print(this.CurrentArea.GlobalCenter);
        GD.Print(this.Camera.GlobalPosition);
    }
}
