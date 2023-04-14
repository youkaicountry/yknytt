using Godot;
using System;

public partial class GhostWalker : Muff
{
    [Export] protected float wallSpeed = 5;

    bool inWall = false;
    protected GhostMod ghostMod;

    public override void _Ready()
    {
        base._Ready();
        ghostMod = GetNode<GhostMod>("GhostMod");
    }

    private void _on_Area2D_body_entered(Node2D body)
    {
        inWall = true;
        changeSpeed(wallSpeed);
        ghostMod.flickerMax = .05f;
        ghostMod.flickerMin = 0f;
    }

    private void _on_Area2D_body_exited(Node2D body)
    {
        if (GetNode<Area2D>("Area2D").GetOverlappingBodies().Count > 1) { return; }
        inWall = false;
        ghostMod.flickerMax = .4f;
        ghostMod.flickerMin = .2f;
    }

    protected override void _on_SpeedTimer_timeout()
    {
        if (!inWall) { base._on_SpeedTimer_timeout(); }
    }
}
