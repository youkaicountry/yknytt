using Godot;

public class RollerGenerator : GDKnyttBaseObject
{
    public override void _Ready()
    {
        var bulletScene = ResourceLoader.Load<PackedScene>("res://knytt/objects/bullets/RollBullet.tscn");
        GDArea.Bullets.RegisterEmitter(this, 15,
            () => bulletScene.Instance() as BaseBullet,
            (p, i) => 
            {
                p.VelocityMMF2 = 12;
                p.DirectionMMF2 = 16;
            });
        GetNode<Timer>("FirstShotTimer").Start();
        GetNode<Timer>("FirstDelayTimer").Start();
        GDArea.Selector.Register(this);
    }

    private void _on_FirstShotTimer_timeout()
    {
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
    }

    private void _on_FirstDelayTimer_timeout()
    {
        GetNode<Timer>("TotalTimer").Start();
        _on_TotalTimer_timeout();
    }

    private void _on_TotalTimer_timeout()
    {
        if (GDArea.Selector.IsObjectSelected(this))
        {
            GDArea.Bullets.Emit(this);
            GetNode<Timer>("Sound1Timer").Start();
        }
    }

    private void _on_Sound1Timer_timeout()
    {
        GetNode<AudioStreamPlayer2D>("PreparePlayer").Play();
        GetNode<Timer>("Sound2Timer").Start();
    }

    private void _on_Sound2Timer_timeout()
    {
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
    }
}
