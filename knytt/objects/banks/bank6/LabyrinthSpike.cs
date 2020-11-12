using Godot;
using System;
using YUtil.Random;

public class LabyrinthSpike : GDKnyttBaseObject
{
    protected Vector2 direction = Vector2.Up;

    [Export] protected float speed = 50f;

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        var collision = Call("move_and_collide", delta * speed * direction, true, true, true) as KinematicCollision2D;
        if (collision != null)
        {
            if (collision.Collider is Juni juni)
            {
                // TODO: Could get stuck in textures if collides both Juni and a wall
                juni.die();
            }
            else
            {
                int retry = 5;
                while (collision != null)
                {
                    direction = direction == Vector2.Up || direction == Vector2.Down ?
                        (GDKnyttDataStore.random.NextBoolean() ? Vector2.Left : Vector2.Right) :
                        (GDKnyttDataStore.random.NextBoolean() ? Vector2.Down : Vector2.Up);
                    collision = Call("move_and_collide", delta * speed * direction, true, true, true) as KinematicCollision2D;
                    if (retry-- <= 0) { return; }
                }
                onCollide();
                GetNode<AudioStreamPlayer2D>("HitSound2D").Play();
            }
        }
        Translate(delta * speed * direction);
    }

    protected virtual void onCollide() {}
}
