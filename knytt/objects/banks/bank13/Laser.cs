using Godot;
using System;
using System.Collections.Generic;

public class Laser : GDKnyttBaseObject
{
    [Export] bool horizontal = false;
    [Export] bool alwaysOn = false;
    [Export] bool onAtStart = false;

    private Timer timer;
    private AnimatedSprite sprite;
    private AudioStreamPlayer2D player;
    private CollisionShape2D collisionShape;
    private bool is_on;
    
    public override void _Ready()
    {
        timer = GetNode<Timer>("SwitchTimer");
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        player = GetNode<AudioStreamPlayer2D>("SwitchPlayer");
        collisionShape = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
        var area = GetNode<Area2D>("Area2D");
        
        if (horizontal)
        {
            area.RotationDegrees = 90;
            sprite.RotationDegrees = 90;
        }
        
        is_on = onAtStart || alwaysOn;
        sprite.Play(is_on ? "on" : "off");
        collisionShape.SetDeferred("disabled", !is_on);
        if (!alwaysOn) { timer.Start(0.8f); }
    }

    private void _on_SwitchTimer_timeout()
    {
        is_on = !is_on;
        sprite.Play(is_on ? "on" : "off");
        collisionShape.SetDeferred("disabled", !is_on);
        // Is it ok to play the same sound for a lot of objects?
        player.Play();
        timer.Start(1.6f);
    }
    
    private void _on_Area2D_body_entered(object body)
    {
        if (body is Juni juni) { juni.die(); }
    }
}

