using Godot;
using YUtil.Random;

public class Waterfall : GDKnyttBaseObject
{
    public override void _Ready()
    {
        var player = GetNode<AnimatedSprite>("AnimatedSprite");
        player.SpeedScale = GDKnyttDataStore.random.NextFloat(.8f, 1.2f);
        player.Play(string.Format("Waterfall{0}", ObjectID.y));
    }
}
