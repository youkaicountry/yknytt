using System;
using Godot;

public class WaterBlock : GDKnyttBaseObject
{
    protected override void _impl_initialize()
    {
        GetNode<AnimatedSprite>("AnimatedSprite").Play(string.Format("Block{0}", ObjectID.y));
    }

    protected override void _impl_process(float delta)
    {
    }
}
