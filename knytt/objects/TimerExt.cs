using Godot;

public class TimerExt : Timer
{
    [Signal] public delegate void timeout_ext();

    // Delay before firing first timeout_ext event. Set firstDelay = 0 to start main timer instead.
    [Export] float firstDelay = 0;

    // Set firstDelay to random value [0..WaitTime], shared for all objects of the same type
    [Export] bool syncedRandomDelay = false;

    // Fire timeout_ext only for one object of the type (and change it every loop)
    [Export] bool pickOne = false;

    // Do not add object for selection with pickOne automatically
    [Export] bool manualRegister = false;

    private GDKnyttBaseObject parent;

    public override void _Ready()
    {
        bool _autostart = Autostart;
        Autostart = false;
        base._Ready();

        if (syncedRandomDelay || pickOne) { parent = GetParent<GDKnyttBaseObject>(); }
        if (syncedRandomDelay) { firstDelay = parent.GDArea.Selector.GetRandomValue(parent, WaitTime); }
        if (pickOne && !manualRegister) { parent.GDArea.Selector.Register(parent); }

        if (_autostart) { RunTimer(); }
    }

    public void RunTimer(float first_delay = -1, float time_sec = -1)
    {
        var first_delay_timer = GetNode<Timer>("FirstDelayTimer");
        first_delay_timer.Stop();
        Stop();

        if (first_delay != -1) { firstDelay = first_delay; }
        if (time_sec != -1) { WaitTime = time_sec; }

        if (firstDelay != 0)
        {
            first_delay_timer.Start(firstDelay);
        }
        else
        {
            base.Start();
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
