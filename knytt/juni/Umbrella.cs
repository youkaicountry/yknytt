using Godot;

public class Umbrella : AnimatedSprite
{
    public bool FacingRight
    {
        get { return !FlipH; }
        set
        {
            this.FlipH = !value;
            var pos = Position;
            pos.x = value ? 3.7f : -4.32f;
            this.Position = pos;
        }
    }

    bool _deployed;
    public bool Deployed
    {
        get { return _deployed; }
        set 
        { 
            if (_deployed) { stow(); }
            else { deploy(); }
        }
    }

    private void deploy()
    {
        this.Visible = true;
        this.Frame = 0;
        _deployed = true;
        Play("Open");
        GetNode<AudioStreamPlayer2D>("../Audio/UmbrellaOpenPlayer2D").Play();
    }

    private async void stow()
    {
        _deployed = false;
        this.Frame = 7;
        Play("Open", true);
        GetNode<AudioStreamPlayer2D>("../Audio/UmbrellaClosePlayer2D").Play();
        var timer = GetNode<Timer>("CloseTimer");
        timer.Start();
        await ToSignal(timer, "timeout");
        if (_deployed) { return; }
        this.Visible = false;
    }

    public void reset()
    {
        this._deployed = false;
        this.Visible = false;
    }
}
