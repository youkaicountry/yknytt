using Godot;

public class FlyBot : GDKnyttBaseObject
{
    PathFollow2D path;

    protected override void _impl_initialize()
    {
        path = GetNode<PathFollow2D>("Path2D/PathFollow2D");
    }

    protected override void _impl_process(float delta)
    {
        path.Offset += 80f * delta;
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni)) { return; }
        Juni.die();
    }
}
