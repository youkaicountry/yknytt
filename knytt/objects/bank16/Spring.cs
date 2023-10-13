using Godot;
using static YKnyttLib.JuniValues;

public class Spring : GDKnyttBaseObject
{
    Juni juni;

    public override void _Ready()
    {
        GetNode<Sprite>("Sprite").Modulate *= new Color(1, 1, 1, 0);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (juni == null) { return; }
        if (juni.GlobalPosition.y > GlobalPosition.y + 10f) { return; }
        if (juni.CurrentState is JumpState || juni.CurrentState is SlideState || juni.CurrentState is ClimbState) { return; }

        // Restore Juni's position to the top of the spring for stability
        juni.GlobalPosition -= new Vector2(0, juni.Bottom.y - GlobalPosition.y);
        juni.Grounded = false;

        // Spring
        // TODO: make Juni.SkipHandleGravityOnce and adjust these velocities
        bool highjump_hold = juni.Powers.getPower(PowerNames.HighJump) && juni.juniInput.JumpHeld;
        juni.executeJump(juni.Swim ? -100f : highjump_hold ? -324f : -346f, sound: false, reset_jumps: true);
        juni.playSound("bounce");

        var anim = GetNode<AnimationPlayer>("AnimationPlayer");
        anim.Stop();
        anim.Play("Spring");
        juni = null;
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (body is Juni juni) { this.juni = juni; }
    }

    public void _on_Area2D_body_exited(Node body)
    {
        if (body is Juni juni && juni == this.juni) { this.juni = null; }
    }
}
