using Godot;

public partial class KeyDoor : Door
{
    [Export] YKnyttLib.JuniValues.PowerNames power = 0;

    protected override bool checkKey(Juni juni)
    {
        return Juni.Powers.getPower(power);
    }

    protected override SignalAwaiter playAnimation()
    {
        var player = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        player.Play("open");
        return ToSignal(player, "animation_finished");
    }
}
