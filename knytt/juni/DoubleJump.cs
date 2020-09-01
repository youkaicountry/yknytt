using Godot;

public class DoubleJump : AnimatedSprite
{
    public override void _Ready()
    {
        GD.Print("DJ EFFECT");
        doEffects();
    }

    public async void doEffects()
    {
        this.Play("DoubleJump");
        await ToSignal(this, "animation_finished");
        GD.Print("DJ EFFECT OVER");
        QueueFree();
    }
}
