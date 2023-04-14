using Godot;

public partial class SimpleDecoration : GDKnyttBaseObject
{
    public override void _Ready()
    {
        base._Ready();
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("" + ObjectID.y);
    }
}
