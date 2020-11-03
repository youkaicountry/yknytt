using Godot;
using System;

public class Spikes : GDKnyttBaseObject
{
    [Export] int openDistance = 70;
    [Export] int closeDistance = 110;
    
    protected bool isOpen = false;
    protected Vector2 center;
    protected AnimatedSprite sprite;
    protected AudioStreamPlayer2D upPlayer;
    protected AudioStreamPlayer2D downPlayer;
    
    public override void _Ready()
    {
        center = GetNode<CollisionShape2D>("Area2D/CollisionShape2D").GlobalPosition;
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        upPlayer = GetNode<AudioStreamPlayer2D>("UpPlayer");
        downPlayer = GetNode<AudioStreamPlayer2D>("DownPlayer");
    }
    
    public override void _PhysicsProcess(float delta)
    {
        if (Juni.dead) { return; }
        
        if (!isOpen && Juni.distance(Center) < openDistance) { updateSpikes(show: true); }
        if (isOpen && Juni.distance(Center) > closeDistance) { updateSpikes(show: false); }
    }    

    private void _on_Area2D_body_entered(object body)
    {
        if (!(body is Juni juni)) { return; }
        if (juni.Hologram == null)
        {
            juni.die();
        }
        else
        {
            juni.Connect(nameof(Juni.HologramChanged), this, nameof(hologramChanged));
        }
    }

    private void _on_Area2D_body_exited(object body)
    {
        if (!(body is Juni juni)) { return; }
        if (juni.IsConnected(nameof(Juni.HologramChanged), this, nameof(hologramChanged)))
        {
            juni.Disconnect(nameof(Juni.HologramChanged), this, nameof(hologramChanged));
        }
    }
    
    public void hologramChanged(Juni juni, bool deployed)
    {
        if (!deployed)
        {
            updateSpikes(show: true);
            juni.die();
        }
    }
    
    protected void updateSpikes(bool show)
    {
        isOpen = show;
        sprite.Play("open", backwards: !show);
        (show ? upPlayer : downPlayer).Play();
    }
}
