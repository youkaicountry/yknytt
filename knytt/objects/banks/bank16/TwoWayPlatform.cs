using Godot;

public class TwoWayPlatform : OneWayPlatform
{
    private CollisionShape2D noJumpShape;

    public override void _Ready()
    {
        noJumpShape = GetNode<CollisionShape2D>("NoJumpArea/CollisionShape2D");
    }

    public override void _PhysicsProcess(float delta)
    {
        // TODO: use local Juni instead of global Juni
        if (Juni.DownHeld)
        { 
            if (noJumpShape.Disabled) { noJumpShape.SetDeferred("disabled", false); }
            if (Juni.JumpEdge) { disablePlatform(true); }
        }
        else
        {
            if (!noJumpShape.Disabled) { noJumpShape.SetDeferred("disabled", true); }
        }
    }
}
