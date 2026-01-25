using Godot;

public class SimpleDecoration : GDKnyttBaseObject
{
    public override void _Ready()
    {
        base._Ready();
        var sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        if (ObjectID.Y == 12) { sprite.Material = null; }
        sprite.Play(ObjectID.y.ToString());
    }
}
