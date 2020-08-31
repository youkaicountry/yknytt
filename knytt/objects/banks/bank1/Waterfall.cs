using Godot;
using YUtil.Random;

public class Waterfall : GDKnyttBaseObject
{
    protected override void _impl_initialize()
    {
        var player = GetNode<AnimatedSprite>("AnimatedSprite");
        player.SpeedScale = GDKnyttDataStore.random.NextFloat(.8f, 1.2f);
        player.Play(string.Format("Waterfall{0}", ObjectID.y));
    }

    protected override void _impl_process(float delta) { }
}
