using Godot;
using System;
using YUtil.Random;

// TODO: KinematicBody2D is more complicated, but doesn't have rattled effect
public class LabyrinthSpike : GDKnyttBaseObject
{
    protected Vector2 direction = Vector2.Up;
    protected Vector2 new_direction;

    [Export] protected float speed = 50f;
    
    protected Timer timer;
    
    public override void _Ready()
    {
        timer = GetNode<Timer>("HitTimer");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        Translate(delta * speed * direction);
    }

    private void _on_Area2D_body_entered(Node2D body)
    {
        if (body is Juni juni) { juni.die(); return; }
        new_direction = direction == Vector2.Up || direction == Vector2.Down ?
            (GDKnyttDataStore.random.NextBoolean() ? Vector2.Left : Vector2.Right) :
            (GDKnyttDataStore.random.NextBoolean() ? Vector2.Down : Vector2.Up);
        direction = -direction;
        onCollide();
        timer.Start();
    }
    
    protected virtual void onCollide() {}
    
    private void _on_Area2D_body_exited(object body)
    {
        direction = new_direction;
    }

    private void _on_HitTimer_timeout()
    {
        GetNode<AudioStreamPlayer2D>("HitSound2D").Play();
    }
}
