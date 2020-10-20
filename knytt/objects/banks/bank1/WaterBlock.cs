using Godot;

public class WaterBlock : GDKnyttBaseObject
{
    public override void _Ready()
    {
        GetNode<AnimatedSprite>("AnimatedSprite").Play($"Block{ObjectID.y}");
    }
}
