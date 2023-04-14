using Godot;

public partial class CollectionDoor : Door
{
    [Export] int creaturesRequired = 0;

    protected override bool checkKey(Juni juni)
    {
        return Juni.Powers.getCreaturesCount() >= creaturesRequired;
    }

    protected override SignalAwaiter playAnimation()
    {
        var player = GetNode<AnimationPlayer>("Sprite2D/AnimationPlayer");
        player.Play("open");
        return ToSignal(player, "animation_finished");
    }
}

