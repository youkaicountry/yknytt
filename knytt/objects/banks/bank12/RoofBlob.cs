using Godot;

public partial class RoofBlob : Muff
{
    private bool shootFast = false;

    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "GhostSlimeBullet",
            (p, i) =>
            {
                p.Translate(new Vector2(38 - random.Next(29), 6));
                p.VelocityMMF2 = 0;
                p.DirectionMMF2 = 24;
                p.GravityMMF2 = 10;
                p.DecelerationMMF2 = 10;
                p.ApllyPinballCorrections();
            });
    }

    private void _on_AnimatedSprite_animation_finished()
    {
        if (sprite.Animation == "turn") { sprite.Play("walk"); }
    }

    protected override void changeDirection(int dir)
    {
        base.changeDirection(dir);
        sprite.Play("turn");
    }

    private void _on_ToggleTimer_timeout_ext()
    {
        shootFast = !shootFast;
        GetNode<TimerExt>("ShotTimer").RunTimer(shootFast ? 0.1f : 0.3f, shootFast ? 0.2f : 0.62f);
    }

    private void _on_ShotTimer_timeout_ext()
    {
        GDArea.Bullets.Emit(this);
    }
}
