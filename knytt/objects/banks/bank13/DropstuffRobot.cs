using Godot;

public class DropstuffRobot : Muff
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Selector.Register(this);
        GDArea.Bullets.RegisterEmitter(this, "DropStuff",
            (p, i) => 
            {
                p.Translate(new Vector2(12 + random.Next(9) - 4, 18));
                p.VelocityMMF2 = 0;
                p.DirectionMMF2 = 24;
                p.GravityMMF2 = 10 + random.Next(5);
            });
    }

    private void _on_LoopTimer_timeout_ext()
    {
        GetNode<RawAudioPlayer2D>("Player").Play();
        GetNode<TimerExt>("ShotTimer").RunTimer();
        GetNode<Timer>("StopAttackTimer").Start();
    }

    private void _on_StopAttackTimer_timeout()
    {
        GetNode<Timer>("ShotTimer").Stop();
    }

    private void _on_ShotTimer_timeout_ext()
    {
        if (GDArea.Selector.IsObjectSelected(this))
        {
            GDArea.Bullets.Emit(this);
        }
    }
}
