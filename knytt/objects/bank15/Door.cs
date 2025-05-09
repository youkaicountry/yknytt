using Godot;

public abstract class Door : GDKnyttBaseObject
{
    private async void _on_OpenArea_body_entered(object body)
    {
        if (body is Juni juni && checkKey(juni))
        {
            if (GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").Disabled) { return; }
            GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").SetDeferred("disabled", true);
            juni.playSound("electronic");
            var sprite = GetNode<AnimatedSprite>("AnimatedSprite");
            sprite.Play("open");
            await ToSignal(sprite, "animation_finished");
            QueueFree();
            Deleted = true;
        }
    }

    public abstract bool checkKey(Juni juni);

    public override void makeSafe() { }
}
