using Godot;
using YUtil.Math;

public class PathCreature : GDKnyttBaseObject
{
    [Export] protected float speed = 30f;

    protected PathFollow2D path;
    protected AnimatedSprite sprite;
    protected Timer flipTimer;
    protected Timer runTimer;
    protected Area2D wallChecker;

    protected bool forward = true;
    protected bool stopped = true;

    public override void _Ready()
    {
        base._Ready();
        path = GetNode<PathFollow2D>("PathFollow2D");
        sprite = path.GetNode<AnimatedSprite>("AnimatedSprite");
        flipTimer = GetNodeOrNull<Timer>("FlipTimer");
        runTimer = GetNodeOrNull<Timer>("RunTimer");
        wallChecker = GetNodeOrNull<Area2D>("PathFollow2D/WallChecker");
        if (runTimer != null) { runTimer.Start(); } else { stop(false); }
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (stopped) { return; }

        bool end_reached = path.UnitOffset.AlmostEqualsWithAbsTolerance(forward ? 1f : 0f, delta * .05f);
        if (end_reached)
        {
            path.UnitOffset = forward && !path.Loop ? 1 : 0;
            if (!path.Loop) { forward = !forward; }

            if (flipTimer != null) { flipTimer.Start(); stop(true); return; }
            if (runTimer != null) { runTimer.Start(); stop(true); return; }
        }

        bool collided = wallChecker?.GetOverlappingBodies().Count > 0;
        // TODO: push out like in KS+, to not get stuck
        if (collided) { forward = !forward; }

        var old_pos = path.GlobalPosition;
        path.Offset += speed * delta * (forward ? 1f : -1f);

        var new_pos = path.GlobalPosition;
        if (!new_pos.x.AlmostEqualsWithAbsTolerance(old_pos.x, delta * .01f))
        {
            sprite.FlipH = (new_pos.x < old_pos.x);
        }
    }

    private void _on_FlipTimer_timeout()
    {
        sprite.FlipH = !sprite.FlipH;
        if (runTimer != null) { runTimer.Start(); } else { stop(false); }
    }

    private void _on_RunTimer_timeout()
    {
        stop(false);
    }

    private void stop(bool stop)
    {
        stopped = stop;
        sprite.Play(stop ? "stop" : "default");
    }
}
