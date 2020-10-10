using Godot;

public class ClimbCheckers : Node2D
{
    public Area2D RightChecker { get; private set; }
    public Area2D LeftChecker { get; private set; }

    public bool RightColliding { get { return RightChecker.GetOverlappingBodies().Count > 0 && RightChecker.GetOverlappingAreas().Count == 0; } }
    public bool LeftColliding { get { return LeftChecker.GetOverlappingBodies().Count > 0 && LeftChecker.GetOverlappingAreas().Count == 0; } }

    public override void _Ready()
    {
        RightChecker = GetNode<Area2D>("RightChecker");
        LeftChecker = GetNode<Area2D>("LeftChecker");
    }
}
