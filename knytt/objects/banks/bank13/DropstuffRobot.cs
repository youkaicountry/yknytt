using Godot;
using System;

public class DropstuffRobot : Muff
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Selector.Register(this);
        GDArea.Bullets.RegisterEmitter(this, "DropStuff", 500,
            (p, i) => 
            {
                p.Translate(new Vector2(12 + GDKnyttDataStore.random.Next(9) - 4, 18));
                p.VelocityMMF2 = 0;
                p.DirectionMMF2 = 24;
                p.GravityMMF2 = 10 + GDKnyttDataStore.random.Next(5);
            });
    }

    private void _on_FirstShotDelayTimer_timeout()
    {
        _on_ShotDelayTimer_timeout();
    }

    private void _on_FirstDelayTimer_timeout()
    {
        GetNode<Timer>("LoopTimer").Start();
        _on_LoopTimer_timeout();
    }

    private void _on_LoopTimer_timeout()
    {
        GetNode<Timer>("ShotTimer").Stop();
        GetNode<Timer>("ShotDelayTimer").Start();
    }

    private void _on_ShotDelayTimer_timeout()
    {
        GetNode<AudioStreamPlayer2D>("Player").Play(0.4f);
        GetNode<Timer>("ShotTimer").Start();
        _on_ShotTimer_timeout();
    }

    private void _on_ShotTimer_timeout()
    {
        if (GDArea.Selector.IsObjectSelected(this))
        {
            GDArea.Bullets.Emit(this);
        }
    }
}
