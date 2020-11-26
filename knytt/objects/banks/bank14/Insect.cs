using Godot;

public class Insect : Muff
{
    [Export] float firstDelay = 0;
    [Export] float timer = 0;
    [Export] DirectionChange runTo = DirectionChange.Random;

    protected override void init()
    {
        base.init();
        GetNode<Timer>("FirstDelayTimer").Start(firstDelay);
    }

    private void _on_FirstDelayTimer_timeout()
    {
        GetNode<Timer>("SpeedTimer").Start(timer);
        _on_SpeedTimer_timeout();
    }

    protected override void _on_SpeedTimer_timeout()
    {
        base._on_SpeedTimer_timeout();
        changeDirection(getDirection(runTo));
    }

    protected override void _on_DirectionTimer_timeout()
    {
        if (speed == 0) { base._on_DirectionTimer_timeout(); }
    }
}
