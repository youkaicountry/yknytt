using Godot;

public abstract partial class BaseWaterMonster : GDKnyttBaseObject
{
    [Export] int bullets = 0;

    protected virtual async void _on_ShotTimer_timeout_ext()
    {
        var sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        sprite.Play("prepare");
        await ToSignal(sprite, "animation_finished");
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
        for (int i = 0; i < bullets; i++) { GDArea.Bullets.Emit(this, i); }
        sprite.Play("aftershot");
    }
}
