using Godot;

public class Laser : GDKnyttBaseObject
{
    private bool[] horizontal = { true, true, true, false, false, false };
    private bool[] alwaysOn = { false, false, true, false, false, true };
    private bool[] onAtStart = { false, true, false, false, true, false };

    private AnimatedSprite sprite;
    private CollisionShape2D collisionShape, climbCollisionShape;
    private bool is_on;

    public override void _Ready()
    {
        int index = ObjectID.y - 7;
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        collisionShape = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
        climbCollisionShape = horizontal[index] ? null : GetNode<CollisionShape2D>("ClimbArea2D/CollisionShape2D");

        if (horizontal[index])
        {
            GetNode<Area2D>("Area2D").RotationDegrees = 90;
            if (!CustomAnimation) { sprite.RotationDegrees = 90; }
        }

        is_on = onAtStart[index] || alwaysOn[index];
        sprite.Play("on");
        sprite.Visible = is_on;
        collisionShape.SetDeferred("disabled", !is_on);
        climbCollisionShape?.SetDeferred("disabled", !is_on);
        if (!alwaysOn[index])
        {
            GetNode<TimerExt>("SwitchTimer").RunTimer();
            GDArea.Selector.Register(this, by_type: true);
        }
    }

    private void _on_SwitchTimer_timeout_ext()
    {
        is_on = !is_on;
        sprite.Visible = is_on;
        collisionShape.SetDeferred("disabled", !is_on);
        climbCollisionShape?.SetDeferred("disabled", !is_on);
        if (GDArea.Selector.IsObjectSelected(this, by_type: true))
        {
            GetNode<AudioStreamPlayer2D>("SwitchPlayer").Play();
        }
    }

    private void _on_ClimbArea2D_body_entered(Juni juni)
    {
        if (juni.CurrentState is ClimbState || juni.CurrentState is SlideState) { juniDie(juni); }
    }
}
