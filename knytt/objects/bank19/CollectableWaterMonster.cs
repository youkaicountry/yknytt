using Godot;

public partial class CollectableWaterMonster : WaterMonster
{
    public override void _Ready()
    {
        base._Ready();
        turn(false);
    }

    protected override void _on_ShotTimer_timeout_ext()
    {
        turn(true);
        base._on_ShotTimer_timeout_ext();
    }

    private void _on_AnimatedSprite_animation_finished()
    {
        if (GetNode<AnimatedSprite2D>("AnimatedSprite2D").Animation == "aftershot") { turn(false); }
    }

    private void turn(bool on)
    {
        GetNode<Area2D>("CollectMod/Area2D").SetDeferred("monitoring", on);
    }
}
