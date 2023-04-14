using Godot;

public partial class TrapCeiling : GDKnyttBaseObject
{
    const float DOWN_SPEED = 100f;
    const float UP_SPEED = 50f;

    bool moving_up = true;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        var move_speed = (float)delta * (moving_up ? -UP_SPEED : DOWN_SPEED);
        Translate(new Vector2(0, move_speed));
    }

    private void _on_Checker_body_entered(Node2D body, bool up)
    {
        moving_up = up;
        GetNode<AudioStreamPlayer2D>("HitSound2D").Play();
    }
}
