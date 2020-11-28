using Godot;

public abstract class ShockDisk : Muff
{
    public override void _Ready()
    {
        base._Ready();
        
        GDArea.Selector.Register(this);
        registerEmitter();

        var first_delay = GDArea.Selector.GetRandomValue(this, GetNode<Timer>("ShotTimer").WaitTime);
        GetNode<Timer>("FirstDelayTimer").Start(first_delay);
    }

    protected abstract void registerEmitter();

    private void _on_FirstDelayTimer_timeout()
    {
        GetNode<Timer>("ShotTimer").Start();
        _on_ShotTimer_timeout();
    }

    private void _on_ShotTimer_timeout()
    {
        if (!GDArea.Selector.IsObjectSelected(this)) { return; }
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
        for (int i = 0; i < 17; i++)
        {
            GDArea.Bullets.Emit(this, i);
        }
    }
}
