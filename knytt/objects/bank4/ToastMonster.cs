public class ToastMonster : GesturesObject
{
    private void _on_SpikerMod_EnterEvent()
    {
        timer.Stop();
    }

    private void _on_SpikerMod_ExitEvent()
    {
        timer.Start();
    }
}
