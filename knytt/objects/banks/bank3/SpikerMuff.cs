public class SpikerMuff : Muff
{
    protected SpikerMod spikerMod;

    public override void _Ready()
    {
        spikerMod = GetNode<SpikerMod>("SpikerMod");
        spikerMod.globalJuni = Juni;
        base._Ready();
    }

    protected override void _on_SpeedTimer_timeout()
    {
        if (!spikerMod.IsEntered) { base._on_SpeedTimer_timeout(); }
    }

    protected override void _on_DirectionTimer_timeout()
    {
        if (!spikerMod.IsEntered) { base._on_DirectionTimer_timeout(); }
    }

    private void _on_SpikerMod_EnterEvent()
    {
        speed = 0;
    }

    // TODO: change speed in CloseEvent
}
