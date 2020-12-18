using Godot;

public class TimerExt : Timer
{
    [Signal] public delegate void timeout_ext();

    // Delay before firing first timeout_ext event. Set firstDelay = 0 to fire first timeout_ext immediately.
    [Export] float firstDelay = 0;
    
    // Set firstDelay to random value [0..WaitTime], shared for all objects of the same type
    [Export] bool syncedRandomDelay = false;

    // Fire timeout_ext only for one object of the type (and change it every loop)
    [Export] bool pickOne = false;

    private GDKnyttBaseObject parent;

    public override void _Ready()
    {
        bool _autostart = Autostart;
        Autostart = false;
        base._Ready();

        if (syncedRandomDelay || pickOne) { parent = GetParent<GDKnyttBaseObject>(); }
        if (syncedRandomDelay)            { firstDelay = parent.GDArea.Selector.GetRandomValue(parent, WaitTime); }
        if (pickOne)                      { parent.GDArea.Selector.Register(parent); }

        if (_autostart) { RunTimer(); }
    }

    public void RunTimer(float first_delay = -1, float time_sec = -1)
    {
        if (first_delay != -1) { firstDelay = first_delay; }
        if (time_sec != -1) { WaitTime = time_sec; }

        if (firstDelay != 0)
        {
            GetNode<Timer>("FirstDelayTimer").Start(firstDelay);
        }
        else
        {
            _on_FirstDelayTimer_timeout();
        }
    }

    private void _on_FirstDelayTimer_timeout()
    {
        _on_Timer_timeout();
        base.Start();
    }

    private void _on_Timer_timeout()
    {
        if (!pickOne || parent.GDArea.Selector.IsObjectSelected(parent))
        {
            EmitSignal(nameof(timeout_ext));
        }
    }

    public new void Start(float _ = 0) { GD.Print("Use RunTimer!"); }
}
