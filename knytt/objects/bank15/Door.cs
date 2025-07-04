using Godot;

public abstract class Door : GDKnyttBaseObject
{
    private Juni juni;

    private async void _on_OpenArea_body_entered(object body)
    {
        if (body is Juni) { juni = (Juni)body; }
        if (checkKey(juni))
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

    private void _on_OpenArea_body_exited(object body)
    {
        if (body == juni) { juni = null; }
    }
    
    public abstract bool checkKey(Juni juni);

    public void gotKey()
    {
        if (juni != null) { _on_OpenArea_body_entered(juni); }
    }

    public override void makeSafe() { }
}
