using Godot;
using YUtil.Random;

public class UpCannon : GDKnyttBaseObject
{
    private AnimatedSprite sprite;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");

        GDArea.Bullets.RegisterEmitter(this, "BlueBullet",
            (p, i) => 
            {
                p.Modulate = new Color(2, 2, 2, 0.75f);
                p.Translate(new Vector2(24, 0));
                p.VelocityX = random.NextBoolean() ? 50 : -50;
                p.VelocityY = random.NextBoolean() ? -150 : -200;
                p.Gravity = 250;
            });
    }

    private async void _on_ShotTimer_timeout_ext()
    {
        sprite.Play("shoot");
        await ToSignal(sprite, "animation_finished");
        sprite.Play("default");
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
        GDArea.Bullets.Emit(this);
    }
}
