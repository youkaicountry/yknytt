using Godot;

public class SpikerMod : Node2D
{
    enum DistanceMethod { Distance, ManhattanDistance, DistanceByX };

    [Export] float openDistance = 0;
    [Export] float closeDistance = 0;
    [Export] DistanceMethod method = DistanceMethod.Distance;
    [Export] NodePath spritePath = null;
    [Export] string openAnimation = "open";
    [Export] NodePath areaPath = null;

    [Signal] public delegate void OpenEvent();
    [Signal] public delegate void CloseEvent();

    private AnimatedSprite sprite;
    private Area2D area;
    private AudioStreamPlayer2D openPlayer;
    private AudioStreamPlayer2D closePlayer;

    public Juni globalJuni { protected get; set; }
    public bool IsOpen { get; protected set; } = false;

    
    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>(spritePath);
        area = GetNode<Area2D>(areaPath);
        openPlayer = GetNode<AudioStreamPlayer2D>("OpenPlayer");
        closePlayer = GetNode<AudioStreamPlayer2D>("ClosePlayer");
        area.Connect("body_entered", this, nameof(_body_entered));
        area.Connect("body_exited", this, nameof(_body_exited));
    }

    public override void _PhysicsProcess(float delta)
    {
        if (globalJuni.dead) { return; }
        
        var distance = 
            method == DistanceMethod.Distance ? globalJuni.distance(GlobalPosition) : 
            method == DistanceMethod.ManhattanDistance ? globalJuni.manhattanDistance(GlobalPosition) :
                Mathf.Abs(GlobalPosition.x - globalJuni.ApparentPosition.x);
                
        if (!IsOpen && distance < openDistance) { updateSpikes(show: true); }
        if (IsOpen && distance > closeDistance) { updateSpikes(show: false); }
    }

    public void _body_entered(Node body)
    {
        if (!(body is Juni juni)) { return; }
        if (IsOpen)
        {
            juni.die();
        }
        else if (juni.Hologram != null)
        {
            juni.Connect(nameof(Juni.HologramStopped), this, nameof(hologramStopped));
        }
    }

    public void _body_exited(Node body)
    {
        if (!(body is Juni juni)) { return; }
        if (juni.IsConnected(nameof(Juni.HologramStopped), this, nameof(hologramStopped)))
        {
            juni.Disconnect(nameof(Juni.HologramStopped), this, nameof(hologramStopped));
        }
    }

    public void hologramStopped(Juni juni)
    {
        updateSpikes(show: true);
        juni.die();
    }
    
    protected void updateSpikes(bool show)
    {
        IsOpen = show;
        EmitSignal(show ? nameof(OpenEvent) : nameof(CloseEvent));
        sprite.Play(openAnimation, backwards: !show);
        (show ? openPlayer : closePlayer).Play();
    }
}
