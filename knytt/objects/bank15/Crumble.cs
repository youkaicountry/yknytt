using Godot;

public class Crumble : GDKnyttBaseObject
{
    private AnimatedSprite sprite;
    private Timer destroy_timer;
    private Juni juni;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        destroy_timer = GetNode<Timer>("DestroyTimer");
    }

    private void _on_Area2D_body_entered(Juni juni)
    {
        this.juni = juni;
        destroy_timer.Start();
        sprite.Play("activate");
        GetNode<CollisionShape2D>("Area2D/CollisionShape2D").SetDeferred("disabled", true);
    }

    private async void _on_DestroyTimer_timeout()
    {
        sprite.Play("destroy");
        await ToSignal(sprite, "animation_finished");
        GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").SetDeferred("disabled", true);
        sprite.Play("disappear");
        juni = null;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (juni == null || destroy_timer.IsStopped() || !juni.Checkers.IsInside) { return; }
        destroy_timer.Stop();
        GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").SetDeferred("disabled", true);
        sprite.Play("disappear");
        juni = null;
    }

    public override void makeSafe() { }
}
