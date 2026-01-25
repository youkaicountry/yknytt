using Godot;

public class GhostLookMuff : GDKnyttBaseObject
{
    protected AnimatedSprite2D sprite;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        sprite.Play();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (Juni.ApparentPosition.x - 8 > Center.x) { sprite.FlipH = false; }
        if (Juni.ApparentPosition.x + 8 < Center.x) { sprite.FlipH = true; }
    }
}
