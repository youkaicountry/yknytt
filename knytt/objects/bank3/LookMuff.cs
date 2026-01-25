using Godot;

public class LookMuff : GDKnyttBaseObject
{
    protected Sprite2D sprite;

    public override void _Ready()
    {
        sprite = GetNode<Sprite2D>("Sprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (Juni.ApparentPosition.x - 8 > Center.x) { sprite.FlipH = false; }
        if (Juni.ApparentPosition.x + 8 < Center.x) { sprite.FlipH = true; }
    }
}
