using Godot;
using System;

public abstract class Bouncer : GDKnyttBaseObject
{
    [Export] float initialSpeed = 0;
    [Export] float gravity = 0.2f;

    protected float speed;
    protected AnimatedSprite animatedSprite;

    public override void _Ready()
    {
        base._Ready();
        OrganicEnemy = true;
        
        speed = -initialSpeed;
        animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        speed += gravity * 50 * delta;
        Translate(new Vector2(0, speed));
    }

    private void _on_Area2D_body_entered(object body)
    {
        if (body is Juni juni) { juni.die(); return; }

        animatedSprite.Frame = 0;
        animatedSprite.Play("bounce");
        GetNode<AudioStreamPlayer2D>("BouncePlayer").Play();
        speed = -nextSpeed();
    }

    protected abstract float nextSpeed();
}

