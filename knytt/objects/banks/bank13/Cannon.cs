using Godot;

public abstract partial class Cannon : GDKnyttBaseObject
{
    [Export] int bulletsCount = 0;
    [Export] float audioPosition = 0;

    protected Timer bulletTimer;
    protected AudioStreamPlayer2D player;
    protected int counter;

    public override void _Ready()
    {
        bulletTimer = GetNode<Timer>("BulletTimer");
        player = GetNode<AudioStreamPlayer2D>("Player");
        GDArea.Selector.Register(this);
    }

    private void _on_TotalTimer_timeout_ext()
    {
        counter = 0;
        bulletTimer.Start();
        player.Play(audioPosition);
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
