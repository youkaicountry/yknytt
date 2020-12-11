using Godot;
using System;

public abstract class BaseWaterMonsterNew : GDKnyttBaseObject
{
    [Export] int bullets = 0;

    private async void _on_ShotTimer_timeout_ext()
    {
        var sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        sprite.Play("prepare");
        await ToSignal(sprite, "animation_finished");
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
        for (int i = 0; i < bullets; i++) { GDArea.Bullets.Emit(this, i); }
        sprite.Play("aftershot");
    }
}
