using Godot;
using YUtil.Random;

public abstract class Cannon : GDKnyttBaseObject
{
    [Export] int bulletsCount = 0;
    
    protected Timer delayTimer;
    protected AudioStreamPlayer2D player;
    
    public override void _Ready()
    {
        var bulletScene = ResourceLoader.Load<PackedScene>("res://knytt/objects/bullets/DropStuff.tscn");
        delayTimer = GetNode<Timer>("DelayTimer");
        player = GetNode<AudioStreamPlayer2D>("Player");
        GDArea.Bullets.RegisterEmitter(this, 500,
            () => bulletScene.Instance() as BaseBullet,
            (p, i) => reinitializeBullet(p, i));
        _on_TotalTimer_timeout();
    }
    
    private void _on_TotalTimer_timeout()
    {
        delayTimer.Start();
    }

    private void _on_DelayTimer_timeout()
    {
        playSound();
        GDArea.Bullets.EmitOnTimer(GetType(), bulletsCount);
    }

    protected virtual void playSound()
    {
        player.Play();
    }
    
    protected abstract void reinitializeBullet(BaseBullet p, int i);
}
