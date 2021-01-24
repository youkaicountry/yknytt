using Godot;
using System.Collections.Generic;
using System.Linq;

public class TwoWayPlatform : OneWayPlatform
{
    private HashSet<Juni> junis = new HashSet<Juni>();

    public override void _PhysicsProcess(float delta)
    {
        foreach (Juni juni in junis)
        {
            if (juni.juniInput.checkJustPressed("down")) { juni.CanJump = false; }
            if (juni.juniInput.checkJustReleased("down") && !juni.CanJump) { juni.CanJump = true; }
        }

        bool disable_ground = junis.Any(j => j.juniInput.DownHeld && j.juniInput.JumpEdge);
        if (disable_ground) { disablePlatform(true); }
    }

    private void _on_EnterArea_body_entered(Juni juni)
    {
        if (juni.juniInput.DownHeld) { juni.CanJump = false; }
        junis.Add(juni);
    }

    private void _on_EnterArea_body_exited(Juni juni)
    {
        if (juni.juniInput.DownHeld) { juni.CanJump = true; }
        junis.Remove(juni);
    }
}
