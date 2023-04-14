using Godot;

public partial class DoubleJump : AnimatedSprite2D
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
