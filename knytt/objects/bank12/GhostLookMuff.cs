using Godot;

public class GhostLookMuff : GDKnyttBaseObject
{
    protected AnimatedSprite sprite;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        sprite.Play();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (Juni.ApparentPosition.x - 8 > Center.x) { sprite.FlipH = false; }
        if (Juni.ApparentPosition.x + 8 < Center.x) { sprite.FlipH = true; }
    }
}
