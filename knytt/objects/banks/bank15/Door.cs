using Godot;

public abstract partial class Door : GDKnyttBaseObject
{
    protected void _on_OpenArea_body_entered(Node2D body)
    {
        if (body is Juni juni && checkKey(juni)) { open(); }
    }

    protected abstract bool checkKey(Juni juni);

    private async void open()
    {
        GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").SetDeferred("disabled", true);
        GetNode<AudioStreamPlayer2D>("DoorPlayer2D").Play();
        await playAnimation();
        QueueFree();
    }

    protected abstract SignalAwaiter playAnimation();
}

