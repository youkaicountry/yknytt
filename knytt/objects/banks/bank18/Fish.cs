using Godot;
using YUtil.Math;

public class Fish : GDKnyttBaseObject
{
    [Export] protected float speed = 30f;

    PathFollow2D path;
    bool forward = true;
    AnimatedSprite sprite;

    public override void _Ready()
    {
        base._Ready();
        path = GetNode<PathFollow2D>("PathFollow2D");
        sprite = path.GetNode<AnimatedSprite>("AnimatedSprite");
        sprite.Play("default");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if (path.UnitOffset.AlmostEqualsWithAbsTolerance(forward ? 1f : 0f, .000001f) ||
            GetNode<Area2D>("PathFollow2D/Area2D").GetOverlappingBodies().Count > 0)
        {
            // TODO: push out like in KS+, to not get stuck
            forward = !forward;
        }

        var old_pos = path.GlobalPosition;
        path.Offset += speed * delta * (forward ? 1f : -1f);

        var new_pos = path.GlobalPosition;
        sprite.FlipH = (new_pos.x < old_pos.x);
    }
}
