using Godot;

public partial class LookMuff : GDKnyttBaseObject
{
    protected Sprite2D sprite;

    public override void _Ready()
    {
        sprite = GetNode<Sprite2D>("Sprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (Juni.ApparentPosition.X - 8 > Center.X) { sprite.FlipH = false; }
        if (Juni.ApparentPosition.X + 8 < Center.X) { sprite.FlipH = true; }
    }
}
