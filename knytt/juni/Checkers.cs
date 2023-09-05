using Godot;

public class Checkers : Node2D
{
    public bool Disabled 
    { 
        get { return GetNode<CollisionShape2D>("RightClimb/CollisionShape2D").Disabled; }
        set 
        {
            GetNode<CollisionShape2D>("Climb/CollisionShape2D").Disabled = value;
            GetNode<CollisionShape2D>("Bump/CollisionShape2D").Disabled = value;
            GetNode<CollisionShape2D>("InsideDetector/CollisionShape2D").Disabled = value;
        }
    }

    public bool Colliding => GetNode<Area2D>("Climb").GetOverlappingBodies().Count > 0 && 
                             GetNode<Area2D>("Climb").GetOverlappingAreas().Count == 0;

    public bool Bump => GetNode<Area2D>("Bump").GetOverlappingBodies().Count > 0 && 
                        GetNode<Area2D>("Climb").GetOverlappingBodies().Count == 0; 

    int inside = 0; // Number of sticky zones covering Juni
    public bool IsInside
    {
        get { return inside > 0; }
        set { inside += (value ? 1 : -1); }
    }

    public void _on_InsideDetector_body_entered(Node body)
    {
        IsInside = true;
    }

    public void _on_InsideDetector_body_exited(Node body)
    {
        IsInside = false;
    }
}
