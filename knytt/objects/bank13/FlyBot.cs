using Godot;

public partial class FlyBot : GDKnyttBaseObject
{
    PathFollow2D path;

    public override void _Ready()
    {
        base._Ready();
        path = GetNode<PathFollow2D>("PathFollow2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        // Godot 4: Offset → Progress
        path.Progress += 80f * (float)delta;
    }
}
