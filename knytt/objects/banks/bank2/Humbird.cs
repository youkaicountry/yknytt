using Godot;
using System;

public class Humbird : GDKnyttBaseObject
{
    private AnimatedSprite sprite;
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
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        originY = GlobalPosition.y;
        runTimer.Start();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Juni.ApparentPosition.x - 8 > Center.x) { sprite.FlipH = false; }
        if (Juni.ApparentPosition.x + 8 < Center.x) { sprite.FlipH = true; }

        if (runTimer.TimeLeft > 0) { return; }
        
        time += delta;
        float new_y = originY - (1 - Mathf.Abs(Mathf.Pow(1 - time / T, POW))) * H;
        
        if (new_y < originY)
        {
            GlobalPosition = new Vector2(GlobalPosition.x, new_y);
        }
        else
        {
            GlobalPosition = new Vector2(GlobalPosition.x, originY);
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
