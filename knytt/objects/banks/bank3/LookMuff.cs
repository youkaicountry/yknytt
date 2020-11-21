using Godot;

public class LookMuff : GDKnyttBaseObject
{
    protected Sprite sprite;

    public override void _Ready()
    {
        sprite = GetNode<Sprite>("Sprite");
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Juni.ApparentPosition.x - 8 > Center.x) { sprite.FlipH = false; }
        if (Juni.ApparentPosition.x + 8 < Center.x) { sprite.FlipH = true; }
    }
}
