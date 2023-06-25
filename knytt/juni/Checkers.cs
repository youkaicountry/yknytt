using Godot;

public class Checkers : Node2D
{
    public bool Disabled 
    { 
        get { return GetNode<CollisionShape2D>("RightClimb/CollisionShape2D").Disabled; }
        set 
        {
            GetNode<CollisionShape2D>("RightClimb/CollisionShape2D").Disabled = value;
            GetNode<CollisionShape2D>("LeftClimb/CollisionShape2D").Disabled = value;
            GetNode<CollisionShape2D>("RightBump/CollisionShape2D").Disabled = value;
            GetNode<CollisionShape2D>("LeftBump/CollisionShape2D").Disabled = value;
            GetNode<CollisionShape2D>("InsideDetector/CollisionShape2D").Disabled = value;
        }
    }

    public bool RightColliding
    {
        get
        {
            return GetNode<Area2D>("RightClimb").GetOverlappingBodies().Count > 0 && 
                   GetNode<Area2D>("RightClimb").GetOverlappingAreas().Count == 0;
        }
    }

    public bool LeftColliding 
    { 
        get 
        { 
            return GetNode<Area2D>("LeftClimb").GetOverlappingBodies().Count > 0 && 
                   GetNode<Area2D>("LeftClimb").GetOverlappingAreas().Count == 0; 
        }
    }

    public bool RightBump 
    { 
        get 
        { 
            return GetNode<Area2D>("RightBump").GetOverlappingBodies().Count > 0 && 
                   GetNode<Area2D>("RightClimb").GetOverlappingBodies().Count == 0; 
        } 
    }

    public bool LeftBump 
    { 
        get 
        { 
            return GetNode<Area2D>("LeftBump").GetOverlappingBodies().Count > 0 && 
                   GetNode<Area2D>("LeftClimb").GetOverlappingBodies().Count == 0; 
        } 
    }

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
