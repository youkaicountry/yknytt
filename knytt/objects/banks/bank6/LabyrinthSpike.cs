using Godot;
using YUtil.Random;

public class LabyrinthSpike : GDKnyttBaseObject
{
    protected Vector2 direction = Vector2.Up;

    [Export] protected float speed = 50f;

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        var collision = moveAndCollide(delta * speed * direction, testOnly: true);
        if (collision != null)
        {
            int retry = 5;
            while (collision != null)
            {
                direction = direction == Vector2.Up || direction == Vector2.Down ?
                    (random.NextBoolean() ? Vector2.Left : Vector2.Right) :
                    (random.NextBoolean() ? Vector2.Down : Vector2.Up);
                collision = moveAndCollide(delta * speed * direction, testOnly: true);
                if (retry-- <= 0) { return; }
            }
            onCollide();
            GetNode<AudioStreamPlayer2D>("HitSound2D").Play();
        }
        moveAndCollide(delta * speed * direction);
    }

    protected virtual void onCollide() { }
}
