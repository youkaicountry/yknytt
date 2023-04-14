using Godot;

public partial class Crumble : GDKnyttBaseObject
{
    private AnimatedSprite2D sprite;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    private void _on_Area2D_body_entered(Juni juni)
    {
        GetNode<Timer>("DestroyTimer").Start();
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
