using Godot;
using System;

public abstract class StuffShooter : GDKnyttBaseObject
{
    [Export] float bulletSpeed = 0;
    [Export] bool up = false;
    [Export] Vector2 shotPosition = new Vector2(0, 0);
    [Export] int distanceToShot = 0;

    private bool readyToShoot = true;
    private bool alreadyShot = false;
    private AnimatedSprite sprite;
    private AudioStreamPlayer2D player;
    private Timer shotDelayTimer;

    public override void _Ready()
    {
        OrganicEnemy = true;
        
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        player = GetNode<AudioStreamPlayer2D>("ShotPlayer");
        shotDelayTimer = GetNode<Timer>("ShotDelayTimer");

        GDArea.Bullets.RegisterEmitter(this, "BigGlowingBullet", 10,
            (p, i) => 
            {
                p.DisapperarPlayer = GetNode<RawAudioPlayer2D>("HitPlayer");
                p.Translate(shotPosition);
                p.Velocity = bulletSpeed * 50;
                p.Direction = (up ? 1 : 3) * (Mathf.Pi / 2);
            });
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (Juni.dead) { return; }
        if (Mathf.Abs(Juni.ApparentPosition.x - Center.x) < distanceToShot && readyToShoot)
        {
            sprite.Frame = 0;
            sprite.Play("launch");
            readyToShoot = false;
            alreadyShot = false;
            shotDelayTimer.Start();
        }
        if (sprite.IsPlaying() && sprite.Frame >= 2 && !alreadyShot)
        {
            alreadyShot = true;
            player.Play();
            GDArea.Bullets.Emit(this);
        }
    }
    
    private void _on_ShotDelayTimer_timeout()
    {
        readyToShoot = true;
    }
}
