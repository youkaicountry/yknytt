using Godot;

public class BigSpiker : GesturesObject
{
    [Export] Vector2 openOffset = Vector2.Zero;

    private void _on_SpikerMod_EnterEvent()
    {
        sprite.Offset = openOffset;
        timer.Stop();
    }

    private void _on_SpikerMod_ExitEvent()
    {
        timer.Start();
    }

    protected override void nextAnimation()
    {
        sprite.Offset = Vector2.Zero;
        base.nextAnimation();
    }
}
