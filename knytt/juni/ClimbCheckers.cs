using Godot;

public class ClimbCheckers : Node2D
{
    Area2D right_checker;
    Area2D left_checker;

    public bool RightColliding { get { return right_checker.GetOverlappingBodies().Count > 0; } }
    public bool LeftColliding { get { return left_checker.GetOverlappingBodies().Count > 0; } }

    public override void _Ready()
    {
        right_checker = GetNode<Area2D>("RightChecker");
        left_checker = GetNode<Area2D>("LeftChecker");
    }
}
