using Godot;

public class WaterBlock : GDKnyttBaseObject
{
    public override void _Ready()
    {
        GetNode<AnimatedSprite>("AnimatedSprite").Play(string.Format("Block{0}", ObjectID.y));
    }
}
