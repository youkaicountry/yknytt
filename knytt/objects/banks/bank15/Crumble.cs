using Godot;

public class Crumble : GDKnyttBaseObject
{
    private AnimatedSprite sprite;
    private Timer destroyTimer;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        destroyTimer = GetNode<Timer>("DestroyTimer");
    }

    private void _on_Area2D_body_entered(Juni juni)
    {
        if (!destroyTimer.IsStopped()) { return; }
        destroyTimer.Start();
        sprite.Play("activate");
        GetNode<CollisionShape2D>("Area2D/CollisionShape2D").SetDeferred("disabled", true);
    }

    private async void _on_DestroyTimer_timeout()
    {
        sprite.Play("destroy");
        await ToSignal(sprite, "animation_finished");
        GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").SetDeferred("disabled", true);
        sprite.Play("disappear");
    }
}
