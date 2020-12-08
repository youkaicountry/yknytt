using Godot;
using System;

public abstract class BaseWaterMonsterNew : GDKnyttBaseObject
{
    [Export] int bullets;

    public override void _Ready()
    {
        GDArea.Selector.Register(this);
        registerEmitter();
    }

    protected abstract void registerEmitter();

    private void _on_FirstDelayTimer_timeout()
    {
        GetNode<Timer>("ShotTimer").Start();
        _on_ShotTimer_timeout();
    }

    private async void _on_ShotTimer_timeout()
    {
        if (!GDArea.Selector.IsObjectSelected(this)) { return; }

        var sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        sprite.Play("prepare");
        await ToSignal(sprite, "animation_finished");
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
        for (int i = 0; i < bullets; i++) { GDArea.Bullets.Emit(this, i); }
        sprite.Play("aftershot");
    }
}
