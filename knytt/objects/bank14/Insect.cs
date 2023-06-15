using Godot;

public class Insect : Muff
{
    [Export] DirectionChange runTo = DirectionChange.Random;

    private void _on_AttackTimer_timeout_ext()
    {
        _on_SpeedTimer_timeout();
        changeDirection(getDirection(runTo));
    }

    protected override void _on_DirectionTimer_timeout()
    {
        if (speed == 0) { base._on_DirectionTimer_timeout(); }
    }
}
