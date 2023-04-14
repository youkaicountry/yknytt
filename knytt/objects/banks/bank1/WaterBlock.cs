using Godot;

public partial class WaterBlock : GDKnyttBaseObject
{
    public override void _Ready()
    {
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play($"Block{ObjectID.y}");
    }
}
