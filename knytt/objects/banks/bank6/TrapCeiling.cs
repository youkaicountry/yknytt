using Godot;

public class TrapCeiling : GDKnyttBaseObject
{
    const float DOWN_SPEED = 100f;
    const float UP_SPEED = 50f;

    bool moving_up = true;

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        var move_speed = delta * (moving_up ? -UP_SPEED : DOWN_SPEED);
        Translate(new Vector2(0, move_speed));
    }

    public void _on_Area2D_body_entered(Node2D body)
    {
        if (body is Juni juni) { juniDie(juni); return; }

        moving_up = !moving_up;
        GetNode<AudioStreamPlayer2D>("HitSound2D").Play();
    }
}
