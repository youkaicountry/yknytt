using Godot;

public class LockBlock : GDKnyttBaseObject
{
    public bool opened;
    
    public async void open()
    {
        if (opened) { return; }
        opened = true;
        GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").SetDeferred("disabled", true);
        var sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        sprite.Play("open");
        await ToSignal(sprite, "animation_finished");
        QueueFree();
    }    

    public override void makeSafe() { }
}
