using Godot;

public class DoubleJump : AnimatedSprite
{
    public override void _Ready()
    {
        doEffects();
    }

    public async void doEffects()
    {
        this.Play("DoubleJump");
        await ToSignal(this, "animation_finished");
        QueueFree();
    }
}
