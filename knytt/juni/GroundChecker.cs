using Godot;

public class GroundChecker : Area2D
{
    public bool IsOnGround { get { return GetOverlappingBodies().Count > 0; } }

    public int _no_jumps = 0;
    public bool CanJump
    {  
        get { return GetOverlappingAreas().Count == 0; }
    }
}
