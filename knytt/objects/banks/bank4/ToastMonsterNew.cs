public class ToastMonsterNew : GesturesObject
{
    public override void _Ready()
    {
        GetNode<SpikerMod>("SpikerMod").globalJuni = Juni;
        base._Ready();
    }

    private void _on_SpikerMod_EnterEvent()
    {
        timer.Stop();
    }

    private void _on_SpikerMod_ExitEvent()
    {
        timer.Start();
    }
}
