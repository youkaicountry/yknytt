using Godot;
using System;

public partial class Humbird : GDKnyttBaseObject
{
    private AnimatedSprite2D sprite;
    private Timer runTimer;
    private float originY;
    private float time;

    private const float H = 112;
    private const float T = 2.9f;
    private const int POW = 2;

    public override void _Ready()
    {
        base._Ready();
        runTimer = GetNode<Timer>("RunTimer");
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        originY = GlobalPosition.Y;
        runTimer.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Juni.ApparentPosition.X - 8 > Center.X) { sprite.FlipH = false; }
        if (Juni.ApparentPosition.X + 8 < Center.X) { sprite.FlipH = true; }

        if (runTimer.TimeLeft > 0) { return; }
        
        time += (float)delta;
        float new_y = originY - (1 - Mathf.Abs(Mathf.Pow(1 - time / T, POW))) * H;
        
        if (new_y < originY)
        {
            GlobalPosition = new Vector2(GlobalPosition.X, new_y);
        }
        else
        {
            GlobalPosition = new Vector2(GlobalPosition.X, originY);
            runTimer.Start();
            sprite.Play("stop");
        }
    }

    private void _on_RunTimer_timeout()
    {
        sprite.Play("fly");
        time = 0;
    }
}
