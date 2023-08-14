using Godot;

public class SelfDropper : GDKnyttBaseObject
{
    private const int DISTANCE_TO_DROP = 12;
    private float dropSpeed;

    private enum State { Ready, Dropping, Dropped };
    private State state = State.Ready;

    private CollisionShape2D collisionShape;

    public override void _Ready()
    {
        collisionShape = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
        dropSpeed = 0; // 50;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (state == State.Ready && Mathf.Abs(Juni.ApparentPosition.x - Center.x) < DISTANCE_TO_DROP)
        {
            state = State.Dropping;
            GetNode<AnimatedSprite>("AnimatedSprite").Play("launch");
            collisionShape.SetDeferred("disabled", false);
        }
        if (state == State.Dropping)
        {
            if (moveAndCollide(new Vector2(0, delta * dropSpeed)) != null)
            {
                state = State.Dropped;
                collisionShape.SetDeferred("disabled", true);
                GetNode<AudioStreamPlayer2D>("DropPlayer").Play();
            }
            // 14 * 50 * delta -- original formula
            if (dropSpeed < 450) { dropSpeed += 13 * 50 * delta; }
        }
    }

    private void _on_AnimatedSprite_frame_changed()
    {
        int frame = GetNode<AnimatedSprite>("AnimatedSprite").Frame;
        collisionShape.Scale = new Vector2(new float[] { 0.4f, 0.4f, 0.5f, 0.9f, 1f }[frame], 1);
    }
}
