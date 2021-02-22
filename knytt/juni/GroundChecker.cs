using Godot;

public class GroundChecker : Area2D
{
    public bool Disabled 
    { 
        get { return GetNode<CollisionPolygon2D>("CollisionPolygon2D").Disabled; }
        set { GetNode<CollisionPolygon2D>("CollisionPolygon2D").Disabled = value; }
    }

    public bool IsOnGround { get { return GetOverlappingBodies().Count > 0; } }
}
