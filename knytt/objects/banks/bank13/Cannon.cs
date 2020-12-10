using Godot;

public abstract class Cannon : GDKnyttBaseObject
{
    [Export] int bulletsCount = 0;
    
    protected Timer bulletTimer;
    protected RawAudioPlayer2D player;
    protected int counter;
    
    public override void _Ready()
    {
        bulletTimer = GetNode<Timer>("BulletTimer");
        player = GetNode<RawAudioPlayer2D>("Player");
        GDArea.Selector.Register(this);
    }
    
    private void _on_TotalTimer_timeout_ext()
    {
        counter = 0;
        bulletTimer.Start();
        player.Play();
    }
    
    private void _on_BulletTimer_timeout()
    {
        if (GDArea.Selector.IsObjectSelected(this))
        {
            GDArea.Bullets.Emit(this, counter);
        }
        if (++counter >= bulletsCount)
        {
            bulletTimer.Stop();
        }
    }
}
