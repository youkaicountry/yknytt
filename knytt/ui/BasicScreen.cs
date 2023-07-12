using Godot;

public abstract class BasicScreen : CanvasLayer
{
    public abstract void initFocus();

    public void loadScreen(BasicScreen screen)
    {
        ClickPlayer.Play();
        AddChild(screen);
    }

    public virtual void goBack()
    {
        ClickPlayer.Play();
        QueueFree();
        GetParent<BasicScreen>().backEvent();
        GetParent<BasicScreen>().initFocus();
    }

    protected virtual void backEvent() {}
}