using Godot;

public partial class DistanceMod : Node2D
{
    enum DistanceMethod { Distance, ManhattanDistance, DistanceByX };

    [Export] float openDistance = 0;
    [Export] float closeDistance = 0;
    [Export] DistanceMethod method = DistanceMethod.Distance;
    [Export] NodePath spritePath = null;
    [Export] string openAnimation = "open";

    [Signal] public delegate void EnterEventEventHandler();
    [Signal] public delegate void ExitEventEventHandler();

    private AnimatedSprite2D sprite;

    protected GDKnyttBaseObject parent;
    protected Juni globalJuni;
    public bool IsEntered { get; protected set; } = false;


    public override void _Ready()
    {
        parent = GetParent<GDKnyttBaseObject>();
        globalJuni = parent.Juni;
        sprite = spritePath != null ? GetNode<AnimatedSprite2D>(spritePath) : null;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (globalJuni.dead) { return; }

        var distance =
            method == DistanceMethod.Distance ? globalJuni.distance(GlobalPosition) :
            method == DistanceMethod.ManhattanDistance ? globalJuni.manhattanDistance(GlobalPosition) :
                Mathf.Abs(GlobalPosition.X - globalJuni.ApparentPosition.X);

        if (!IsEntered && distance < openDistance) { updateSpikes(show: true); }
        if (IsEntered && distance > closeDistance) { updateSpikes(show: false); }
    }

    protected virtual void updateSpikes(bool show)
    {
        IsEntered = show;
        EmitSignal(show ? SignalName.EnterEvent : SignalName.ExitEvent);
        sprite?.Play(openAnimation, customSpeed: show ? 1 : -1);
    }
}
