using Godot;

public partial class Umbrella : AnimatedSprite2D
{
    public bool FacingRight
    {
        get { return !FlipH; }
        set
        {
            this.FlipH = !value;
            var pos = Position;
            pos.X = value ? 3.7f : -4.32f;
            this.Position = pos;
            umbrellaShape.Scale = new Vector2(value ? 1 : -1, 1);
            umbrellaShape.Position = new Vector2(value ? -3.7f : 3.7f, 0);
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

    private CollisionPolygon2D umbrellaShape;

    public override void _Ready()
    {
        umbrellaShape = GetNode<CollisionPolygon2D>("UmbrellaBody/CollisionPolygon2D");
    }

    private void deploy()
    {
        this.Visible = true;
        this.Frame = 0;
        _deployed = true;
        Play("Open");
        GetNode<AudioStreamPlayer2D>("../Audio/UmbrellaOpenPlayer2D").Play();
        umbrellaShape.SetDeferred("disabled", false);
    }

    private async void stow()
    {
        _deployed = false;
        this.Frame = 7;
        PlayBackwards("Open");
        GetNode<AudioStreamPlayer2D>("../Audio/UmbrellaClosePlayer2D").Play();
        var timer = GetNode<Timer>("CloseTimer");
        timer.Start();
        await ToSignal(timer, "timeout");
        if (_deployed) { return; }
        this.Visible = false;
        umbrellaShape.SetDeferred("disabled", true);
    }

    public void reset()
    {
        this._deployed = false;
        this.Visible = false;
        umbrellaShape.SetDeferred("disabled", true);
    }
}
