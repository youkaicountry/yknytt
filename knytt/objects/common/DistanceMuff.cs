public class DistanceMuff : Muff
{
    protected DistanceMod distanceMod;

    public override void _Ready()
    {
        distanceMod = GetNode<DistanceMod>("DistanceMod");
        base._Ready();
    }

    protected override void _on_SpeedTimer_timeout()
    {
        if (!distanceMod.IsEntered) { base._on_SpeedTimer_timeout(); }
    }

    protected override void _on_DirectionTimer_timeout()
    {
        if (!distanceMod.IsEntered) { base._on_DirectionTimer_timeout(); }
    }

    protected virtual void _on_DistanceMod_EnterEvent()
    {
        speed = 0;
    }

    // TODO: change speed in CloseEvent
}
