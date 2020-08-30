using Godot;

public class Waterfall : GDKnyttBaseObject
{
    protected override void _impl_initialize()
    {
        var player = GetNode<AnimatedSprite>("AnimatedSprite");
        player.SpeedScale = ((float)GDKnyttDataStore.random.NextDouble()) * .4f + .8f;
        player.Play("Waterfall");
    }

    protected override void _impl_process(float delta)
    {
    }
}
