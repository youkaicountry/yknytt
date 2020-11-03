using Godot;

public abstract class GhostObject : GDKnyttBaseObject
{
    public float flickerMin = .1f;
    public float flickerMax = .8f;
    public float flip_time = .065f;
    float flip_counter = 0f;
    float flip_target = 0f;
    public float change_fraction = 8f;

    protected bool Flickering { get; set; }

    private bool _seen;
    protected bool Seen 
    { 
        get { return _seen; } 
        set 
        { 
            _seen = value;
            if (value) { Visible = true; _InvEnable(); }
            else { Visible = false; _InvDisable(); }
        }
    }

    public override void _Ready()
    {
        Flickering = true;
        _InvReady();
        Seen = Juni.Powers.getPower(YKnyttLib.JuniValues.PowerNames.Eye);
    }

    protected virtual void _InvReady() { }
    protected virtual void _InvDisable() { }
    protected virtual void _InvEnable() { }
    protected virtual void _InvProcess(float delta) { }

    public override void _PhysicsProcess(float delta)
    {
        if (!_seen) { return; }
        base._PhysicsProcess(delta);
        if (Flickering) { handleFlicker(delta); }
        _InvProcess(delta);
    }

    private void handleFlicker(float delta)
    {
        flip_counter += delta;
        while(flip_counter > flip_time) 
        { 
            flip_counter -= flip_time;
            this.flip_target = GDKnyttDataStore.random.Next(2) == 0 ? flickerMin : flickerMax;
        }

        var m = Modulate;
        m.a += (flip_target - m.a) * change_fraction * delta;
        Modulate = m;
    }
}
