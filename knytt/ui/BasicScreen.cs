using Godot;

public abstract class BasicScreeen : CanvasLayer
{
    public abstract void initFocus();

    public void loadScreen(BasicScreeen screen)
    {
        ClickPlayer.Play();
        AddChild(screen);
    }

    public virtual void goBack()
    {
        ClickPlayer.Play();
        QueueFree();
        GetParent<BasicScreeen>().backEvent();
        GetParent<BasicScreeen>().initFocus();
    }

    protected virtual void backEvent() {}
}