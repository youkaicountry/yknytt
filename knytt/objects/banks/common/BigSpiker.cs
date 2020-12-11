using Godot;

public class BigSpiker : GesturesObject
{
    [Export] Vector2 openOffset = Vector2.Zero;

    public override void _Ready()
    {
        GetNode<SpikerMod>("SpikerMod").globalJuni = Juni;
        base._Ready();
    }

    private void _on_SpikerMod_EnterEvent()
    {
        gesturesSprite.Offset = openOffset;
        timer.Stop();
    }

    private void _on_SpikerMod_ExitEvent()
    {
        timer.Start();
    }

    protected override void nextAnimation()
    {
        gesturesSprite.Offset = Vector2.Zero;
        base.nextAnimation();
    }
}
