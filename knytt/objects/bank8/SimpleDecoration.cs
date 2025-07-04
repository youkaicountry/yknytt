using Godot;

public class SimpleDecoration : GDKnyttBaseObject
{
    public override void _Ready()
    {
        base._Ready();
        var sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        if (ObjectID.y == 12) { sprite.Material = null; }
        sprite.Play(ObjectID.y.ToString());
    }
}
