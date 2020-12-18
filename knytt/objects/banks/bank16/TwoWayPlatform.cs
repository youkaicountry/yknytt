using Godot;
using System.Collections.Generic;
using System.Linq;

public class TwoWayPlatform : OneWayPlatform
{
    private HashSet<Juni> junis = new HashSet<Juni>();
    private CollisionShape2D noJumpShape;

    public override void _Ready()
    {
        noJumpShape = GetNode<CollisionShape2D>("NoJumpArea/CollisionShape2D");
    }

    public override void _PhysicsProcess(float delta)
    {
        bool enable_nojump = junis.Any(j => j.DownHeld);
        if (!enable_nojump != noJumpShape.Disabled) { noJumpShape.SetDeferred("disabled", !enable_nojump); }

        bool disable_ground = enable_nojump && junis.Any(j => j.DownHeld && j.JumpEdge);
        if (disable_ground) { disablePlatform(true); }
    }

    private void _on_EnterArea_body_entered(Juni juni)
    {
        junis.Add(juni);
    }

    private void _on_EnterArea_body_exited(Juni juni)
    {
        junis.Remove(juni);
    }
}
