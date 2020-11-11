using Godot;
using YUtil.Random;

public abstract class Cannon : GDKnyttBaseObject
{
    [Export] int bulletsCount = 0;
    
    protected Timer delayTimer;
    protected Timer bulletTimer;
    protected AudioStreamPlayer2D player;
    protected int counter;
    
    public override void _Ready()
    {
        var bulletScene = ResourceLoader.Load<PackedScene>("res://knytt/objects/bullets/DropStuff.tscn");
        delayTimer = GetNode<Timer>("DelayTimer");
        bulletTimer = GetNode<Timer>("BulletTimer");
        player = GetNode<AudioStreamPlayer2D>("Player");
        
        GDArea.Bullets.RegisterEmitter(this, 500,
            () => bulletScene.Instance() as BaseBullet,
            (p, i) => reinitializeBullet(p, i));

        GDArea.Selector.Register(this);
        
        _on_TotalTimer_timeout();
    }
    
    private void _on_TotalTimer_timeout()
    {
        delayTimer.Start();
    }

    private void _on_DelayTimer_timeout()
    {
        counter = 0;
        bulletTimer.Start();
        playSound();
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

    protected virtual void playSound()
    {
        player.Play();
    }
    
    protected abstract void reinitializeBullet(BaseBullet p, int i);
}
