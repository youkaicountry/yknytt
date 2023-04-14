using Godot;

public partial class OneWayPlatform : GDKnyttBaseObject
{
    private void _on_Area2D_body_entered(Node2D body)
    {
        disablePlatform(true);
    }

    private void _on_Area2D_body_exited(Node2D body)
    {
        disablePlatform(false);
    }

    protected void disablePlatform(bool disable)
    {
        GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").SetDeferred("disabled", disable);
    }
}
