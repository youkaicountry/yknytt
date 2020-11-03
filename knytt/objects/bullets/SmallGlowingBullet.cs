using Godot;
using System;

public class SmallGlowingBullet : BaseBullet
{
    public override void _PhysicsProcess(float delta)
    {
        if (!Enabled) { return; }
        VelocityMMF2 += delta * 40;
        base._PhysicsProcess(delta);
    }
}
