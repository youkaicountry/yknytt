using Godot;
using YUtil.Random;

public partial class Waterfall : GDKnyttBaseObject
{
    public override void _Ready()
    {
        var player = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        player.SpeedScale = random.NextFloat(.8f, 1.2f);
        player.Play($"Waterfall{ObjectID.y}");
    }
}
