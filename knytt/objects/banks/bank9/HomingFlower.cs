using Godot;

public class HomingFlower : GDKnyttBaseObject
{
    public override void _Ready()
    {
        GDArea.Bullets.RegisterEmitter(this, "HomingBullet",
            (p, i) => 
            {
                (p as HomingBullet).globalJuni = Juni;
                p.DisapperarPlayer = GetNode<RawAudioPlayer2D>("HitPlayer");
                p.Translate(new Vector2(11, 16));
                p.Velocity = 50;
                p.Direction = Mathf.Pi / 2;
            });
    }

    private void _on_LoopTimer_timeout_ext()
    {
        GetNode<AnimatedSprite>("AnimatedSprite").Play("prepare");
        GetNode<Timer>("ShotTimer").Start();
    }

    private void _on_ShotTimer_timeout()
    {
        GetNode<AnimatedSprite>("AnimatedSprite").Play("close");
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
        GDArea.Bullets.Emit(this);
    }
}
