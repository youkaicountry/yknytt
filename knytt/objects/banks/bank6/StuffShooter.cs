using Godot;
using System;

public class StuffShooter : GDKnyttBaseObject
{
    [Export] float bulletSpeed;
    [Export] bool up;
    [Export] Vector2 shotPosition;
    [Export] int distanceToShot;

    private bool ready_to_shoot = true;
    private bool already_shot = false;
    private AnimatedSprite sprite;
    private AudioStreamPlayer2D player;
    private Timer shot_delay_timer;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        player = GetNode<AudioStreamPlayer2D>("ShotPlayer");
        shot_delay_timer = GetNode<Timer>("ShotDelayTimer");

        var bulletScene = ResourceLoader.Load<PackedScene>("res://knytt/objects/bullets/BigGlowingBullet.tscn");
        GDArea.Bullets.RegisterEmitter(this, 10,
            () => bulletScene.Instance() as BaseBullet,
            (p, i) => { p.Translate(shotPosition); p.Velocity = bulletSpeed * 50; p.Direction = (up ? 1 : 3) * (Mathf.Pi / 2); });
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (Juni.dead) { return; }
        if (Mathf.Abs(Juni.ApparentPosition.x - (GlobalPosition.x + 12)) < distanceToShot && ready_to_shoot)
        {
            sprite.Frame = 0;
            sprite.Play("launch");
            ready_to_shoot = false;
            already_shot = false;
            shot_delay_timer.Start();
        }
        if (sprite.IsPlaying() && sprite.Frame >= 2 && !already_shot)
        {
            already_shot = true;
            player.Play();
            GDArea.Bullets.Emit(this);
        }
    }
    
    private void _on_ShotDelayTimer_timeout()
    {
        ready_to_shoot = true;
    }
}
