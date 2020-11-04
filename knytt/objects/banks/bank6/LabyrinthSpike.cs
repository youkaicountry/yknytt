using Godot;
using System;
using YUtil.Random;

// TODO: KinematicBody2D is more complicated, but doesn't have rattled effect
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
                juni.die();
            }
            else
            {
                while (collision != null)
                {
                    direction = direction == Vector2.Up || direction == Vector2.Down ?
                        (GDKnyttDataStore.random.NextBoolean() ? Vector2.Left : Vector2.Right) :
                        (GDKnyttDataStore.random.NextBoolean() ? Vector2.Down : Vector2.Up);
                    collision = Call("move_and_collide", delta * speed * direction, true, true, true) as KinematicCollision2D;
                }
                onCollide();
                GetNode<AudioStreamPlayer2D>("HitSound2D").Play();
            }
        }
        Translate(delta * speed * direction);
    }

    protected virtual void onCollide() {}
}
