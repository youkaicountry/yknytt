using Godot;

public partial class ClimbCheckers : Node2D
{
    public Area2D RightChecker { get; private set; }
    public Area2D LeftChecker { get; private set; }

    public bool Disabled 
    { 
        get { return GetNode<CollisionShape2D>("RightChecker/CollisionShape2D").Disabled; }
        set 
        {
            GetNode<CollisionShape2D>("RightChecker/CollisionShape2D").Disabled = value;
            GetNode<CollisionShape2D>("LeftChecker/CollisionShape2D").Disabled = value;
        }
    }

    public bool RightColliding { get { return RightChecker.GetOverlappingBodies().Count > 0 && RightChecker.GetOverlappingAreas().Count == 0; } }
    public bool LeftColliding { get { return LeftChecker.GetOverlappingBodies().Count > 0 && LeftChecker.GetOverlappingAreas().Count == 0; } }

    public override void _Ready()
    {
        RightChecker = GetNode<Area2D>("RightChecker");
        LeftChecker = GetNode<Area2D>("LeftChecker");
    }
}
