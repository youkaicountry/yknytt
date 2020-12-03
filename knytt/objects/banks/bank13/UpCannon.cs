using Godot;
using YUtil.Random;

public class UpCannon : GDKnyttBaseObject
{
    private AnimatedSprite sprite;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");

        GDArea.Selector.Register(this);
        GDArea.Bullets.RegisterEmitter(this, "BlueBullet", 2,
            (p, i) => 
            {
                p.Modulate = new Color(2, 2, 2, 0.75f);
                p.DisapperarPlayer = GetNode<RawAudioPlayer2D>("HitPlayer");
                p.Translate(new Vector2(24, 0));
                var y_speed = GDKnyttDataStore.random.NextBoolean() ? 3 : 4;
                var x_speed = GDKnyttDataStore.random.NextBoolean() ? 1 : -1;
                p.Velocity = Mathf.Sqrt(y_speed * y_speed + 1) * 50;
                p.Direction = Mathf.Atan2(y_speed, x_speed);
                p.Gravity = 250;
            });
    }

    private void _on_FirstDelayTimer_timeout()
    {
        GetNode<Timer>("ShotTimer").Start();
        _on_ShotTimer_timeout();
    }

    private async void _on_ShotTimer_timeout()
    {
        if (!GDArea.Selector.IsObjectSelected(this)) { return; }
        sprite.Play("shoot");
        await ToSignal(sprite, "animation_finished");
        sprite.Play("default");
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
        GDArea.Bullets.Emit(this);
    }
}
