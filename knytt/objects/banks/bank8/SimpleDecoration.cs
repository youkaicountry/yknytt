using Godot;

public class SimpleDecoration : GDKnyttBaseObject
{
    public override void _Ready()
    {
        base._Ready();
        GetNode<AnimatedSprite>("AnimatedSprite").Play("" + ObjectID.y);
    }
}
