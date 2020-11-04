using Godot;
using System;

public class GhostEater : GhostObject
{
    private BaseEater eater;
    
    public GhostEater()
    {
        eater = new BaseEater();
    }
    
    public override void _Ready()
    {
        OrganicEnemy = true;
        base._Ready();
        Flickering = false;
    }

    private void _on_Area2D_body_entered(object body)
    {
        eater.OnArea2DBodyEntered(this, body);
    }
    
    protected override void _InvEnable()
    {
        GetNode<CollisionShape2D>("Area2D/CollisionShape2D").SetDeferred("disabled", false);
    }

    protected override void _InvDisable()
    {
        GetNode<CollisionShape2D>("Area2D/CollisionShape2D").SetDeferred("disabled", true);
    }
}
