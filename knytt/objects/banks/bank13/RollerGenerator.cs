using Godot;

public class RollerGenerator : GDKnyttBaseObject
{
    public override void _Ready()
    {
        GDArea.Bullets.RegisterEmitter(this, "RollBullet", 15,
            (p, i) => 
            {
                p.DisapperarPlayer = GetNode<RawAudioPlayer2D>("HitPlayer");
                p.VelocityMMF2 = 12;
                p.DirectionMMF2 = 16;
            });
    }

    private void _on_TotalTimer_timeout_ext()
    {
        GDArea.Bullets.Emit(this);
        GetNode<Timer>("Sound1Timer").Start();
        GetNode<Timer>("Sound2Timer").Start();
    }
}
