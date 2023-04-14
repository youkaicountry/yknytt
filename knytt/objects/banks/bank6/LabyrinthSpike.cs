using Godot;
using YUtil.Random;

public partial class LabyrinthSpike : GDKnyttBaseObject
{
    protected Vector2 direction = Vector2.Up;
    protected bool wasFree = false;

    [Export] protected float speed = 50f;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        var collision = moveAndCollide((float)delta * speed * direction, testOnly: !wasFree);
        if (collision != null)
        {
            int retry = 5;
            while (collision != null)
            {
                direction = direction == Vector2.Up || direction == Vector2.Down ?
                    (random.NextBoolean() ? Vector2.Left : Vector2.Right) :
                    (random.NextBoolean() ? Vector2.Down : Vector2.Up);
                collision = moveAndCollide((float)delta * speed * direction, testOnly: true);
                if (retry-- <= 0) { return; }
            }
            onCollide();
            GetNode<AudioStreamPlayer2D>("HitSound2D").Play();
        }
        if (collision == null) { wasFree = true; }
    }

    protected virtual void onCollide() { }
}
