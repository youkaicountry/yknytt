using Godot;
using YUtil.Random;

public class MultiShot : DistanceMuff
{
    [Export] int[] additionalDirections = null;
    [Export] Vector2[] shotPositions = null;

    private Timer shotTimer;

    public override void _Ready()
    {
        base._Ready();
        shotTimer = GetNode<Timer>("ShotTimer");

        GDArea.Bullets.RegisterEmitter(this, "FireBullet", 
            (p, i) =>
            {
                p.Translate(i == -1 ? new Vector2(12, 11) : shotPositions[i]);
                p.VelocityMMF2 = 30 + random.Next(30);
                p.DirectionMMF2 = i == -1 ? 7 + random.Next(3) : additionalDirections[i];
                p.GravityMMF2 = 10;
                p.DecelerationMMF2 = 10;
                p.ApllyPinballCorrections();
            });
    }

    protected override void _on_DistanceMod_EnterEvent()
    {
        base._on_DistanceMod_EnterEvent();
        shotTimer.Start(0.2f);
    }

    private void _on_DistanceMod_ExitEvent()
    {
        shotTimer.Stop();
    }

    private void _on_ShotTimer_timeout()
    {
        GetNode<RawAudioPlayer2D>("ShotPlayer").Play();
        GDArea.Bullets.Emit(this, -1);
        for (int i = 0; i < additionalDirections.Length; i++)
        {
            GDArea.Bullets.Emit(this, i);
        }
        shotTimer.Start(1.2f + random.NextFloat(0.4f));
    }
}
