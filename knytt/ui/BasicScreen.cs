using Godot;

public abstract class BasicScreen : CanvasLayer
{
    public static BasicScreen ActiveScreen;

    public override void _Ready()
    {
        ActiveScreen = this;
    }

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
        ActiveScreen = GetParent<BasicScreen>();
    }

    protected virtual void backEvent() {}

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_back") && ActiveScreen == this && !GetNode<Console>("/root/Console").IsOpen)
        {
            GetTree().SetInputAsHandled();
            goBack();
        }
    }
}