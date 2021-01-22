using Godot;

public class GroundChecker : Area2D
{
    public bool IsOnGround { get { return GetOverlappingBodies().Count > 0; } }

    // This Checks if in a NoJump area
    public int _no_jumps = 0;
    public bool CanJump
    {  
        get { return GetOverlappingAreas().Count == 0; }
    }
}
