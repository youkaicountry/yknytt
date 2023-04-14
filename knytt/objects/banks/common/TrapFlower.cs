using Godot;

public abstract partial class TrapFlower : GDKnyttBaseObject
{
    private void _on_ShotTimer_timeout()
    {
        if (GDArea.Selector.IsObjectSelected(this))
        {
            GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
            GDArea.Bullets.Emit(this);
        }
    }

    private void _on_DistanceMod_EnterEvent()
    {
        GDArea.Selector.Register(this);
        GetNode<Timer>("ShotTimer").Start();
        _on_ShotTimer_timeout();
    }

    private void _on_DistanceMod_ExitEvent()
    {
        GDArea.Selector.Unregister(this);
        GetNode<Timer>("ShotTimer").Stop();
    }
}
