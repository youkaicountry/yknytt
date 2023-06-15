using Godot;

public class RollerGenerator : GDKnyttBaseObject
{
    public override void _Ready()
    {
        GDArea.Bullets.RegisterEmitter(this, "RollBullet",
            (p, i) =>
            {
                p.VelocityMMF2 = 12;
                p.DirectionMMF2 = 0;
            });
    }

    private void _on_TotalTimer_timeout_ext()
    {
        GDArea.Bullets.Emit(this);
        GetNode<Timer>("Sound1Timer").Start();
        GetNode<Timer>("Sound2Timer").Start();
    }
    
    private void _on_Sound1Timer_timeout()
    {
        GetNode<AudioStreamPlayer2D>("PreparePlayer").Play();
    }

    private void _on_Sound2Timer_timeout()
    {
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
    }

    private void _on_FirstShotTimer_timeout()
    {
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
    }
}
