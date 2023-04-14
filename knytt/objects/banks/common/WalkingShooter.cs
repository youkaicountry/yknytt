using Godot;

public partial class WalkingShooter : Muff
{
    [Export] bool waitForTimer = false;
    [Export] int bulletsCount = 0;
    [Export] bool limitBullets = false;
    [Export] bool startWithShoot = false;

    protected bool shoot;

    public override void _Ready()
    {
        base._Ready();
        shoot = !startWithShoot;
        if (limitBullets) { GDArea.Selector.Register(this); }
        _on_ShotTimer_timeout_ext();
    }

    private async void _on_ShotTimer_timeout_ext()
    {
        shoot = !shoot;
        if (shoot)
        {
            _on_DirectionTimer_timeout();

            speed = 0;
            sprite.Play("prepare");

            if (waitForTimer)
            {
                var timer = GetNode<Timer>("ShotDelayTimer");
                timer.Start();
                await ToSignal(timer, "timeout");
            }
            else
            {
                await ToSignal(sprite, "animation_finished");
            }

            GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
            for (int i = 0; i < bulletsCount; i++)
            {
                if (!limitBullets || GDArea.Selector.IsObjectSelected(this))
                {
                    GDArea.Bullets.Emit(this, i);
                }
            }

            if (sprite.SpriteFrames.HasAnimation("aftershot")) { sprite.Play("aftershot"); }
        }
        else
        {
            _on_SpeedTimer_timeout();
        }
    }

    protected int getDirection() { return direction; }
}
