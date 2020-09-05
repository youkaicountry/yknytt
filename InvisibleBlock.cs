using Godot;
using System;

public class InvisibleBlock : InvisibleObject
{
    protected override void _impl_initialize()
    {
        
    }

    protected override void _InvDisable()
    {
        GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").Disabled = true;
    }

    protected override void _InvEnable()
    {
        GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").Disabled = false;
    }
}
