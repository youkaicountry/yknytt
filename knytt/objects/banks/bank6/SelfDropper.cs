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
        dropSpeed = 25;
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
            Translate(new Vector2(0, delta * dropSpeed));
            if (dropSpeed < 450) { dropSpeed += 10 * 50 * delta; }
        }
    }

    private void _on_Area2D_body_entered(object body)
    {
        if (body is Juni juni) { juniDie(juni); return; }
        state = State.Dropped;
        collisionShape.SetDeferred("disabled", true);
        GetNode<AudioStreamPlayer2D>("DropPlayer").Play();
    }
}
