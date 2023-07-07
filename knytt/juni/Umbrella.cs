using Godot;

public class Umbrella : Sprite
{
    public bool FacingRight
    {
        get { return !FlipH; }
        set
        {
            this.FlipH = !value;
            this.Offset = new Vector2(value ? 1 : -1, 0);
            umbrellaShape.Scale = new Vector2(value ? 1 : -1, 1);
        }
    }

    bool _deployed;
    public bool Deployed
    {
        get { return _deployed; }
        set
        {
            if (_deployed && !value) { stow(); }
            if (!_deployed && value) { deploy(); }
        }
    }

    public bool DeployOnFall;

    bool _custom;
    public bool Custom
    {
        get { return _custom; }
        set
        {
            _custom = value;
            if (Deployed) { umbrellaShape.SetDeferred("disabled", _custom); }
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
        _deployed = true;
        GetNode<AnimationPlayer>("AnimationPlayer").Play("deploy");
        GetNode<AudioStreamPlayer2D>("../Audio/UmbrellaOpenPlayer2D").Play();
        umbrellaShape.SetDeferred("disabled", _custom);
    }

    private async void stow()
    {
        _deployed = false;
        GetNode<AnimationPlayer>("AnimationPlayer").Play("stow");
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
