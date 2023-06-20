using Godot;

public class ShadowWitch : GDKnyttBaseObject
{
    private CollisionShape2D collisionShape;
    private AnimatedSprite sprite;

    private float value = 0;

    public override void _Ready()
    {
        collisionShape = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        bool juni_at_right = Juni.ApparentPosition.x > Center.x;
        value += (Juni.ApparentFacingRight && juni_at_right) || (!Juni.ApparentFacingRight && !juni_at_right) ?
            -50 * delta : 50 * delta;
        value = Mathf.Max(Mathf.Min(128, value), 0);

        if (value > 120)
        {
            if (collisionShape.Disabled) { collisionShape.SetDeferred("disabled", false); }
            if (sprite.Animation != "angry") { sprite.Play("angry"); }
        }
        else
        {
            if (!collisionShape.Disabled) { collisionShape.SetDeferred("disabled", true); }
            if (sprite.Animation != "default") { sprite.Play("default"); }
        }
        sprite.FlipH = !juni_at_right;
        sprite.Modulate = new Color(1, 1, 1, value / 128);
    }
}
