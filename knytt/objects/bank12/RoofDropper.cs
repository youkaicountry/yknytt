using Godot;
using YUtil.Random;

public class RoofDropper : GDKnyttBaseObject
{
    private Timer shotTimer;
    private AnimatedSprite sprite;

    public override void _Ready()
    {
        base._Ready();
        shotTimer = GetNode<Timer>("ShotTimer");
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");

        GDArea.Bullets.RegisterEmitter(this, "GhostSlimeBullet",
            (p, i) =>
            {
                p.Translate(new Vector2(11.5f, 5));
                p.VelocityMMF2 = 0;
                p.DirectionMMF2 = 24;
                p.GravityMMF2 = 10;
                p.DecelerationMMF2 = 10;
                p.ApllyPinballCorrections();
            });

        shotTimer.Start(0.8f + random.NextFloat(0.8f));
    }

    private async void _on_ShotTimer_timeout()
    {
        sprite.Play("shot");
        await ToSignal(sprite, "animation_finished");
        GDArea.Bullets.Emit(this);
        sprite.Play("aftershot");
        shotTimer.Start(1 + random.NextFloat(0.8f));
    }
}
