using Godot;

public class GroundChecker : Area2D
{
    public bool IsOnGround { get { return GetOverlappingBodies().Count > 0; } }
}
