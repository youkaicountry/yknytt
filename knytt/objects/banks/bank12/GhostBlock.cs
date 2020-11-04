using Godot;

public class GhostBlock : GhostObject
{
    protected override void _InvReady()
    {
        flickerMax = .1f;
        flickerMin = .6f;
    }

    protected override void _InvDisable()
    {
        GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").SetDeferred("disabled", true);
    }

    protected override void _InvEnable()
    {
        GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").SetDeferred("disabled", false);
    }
}
