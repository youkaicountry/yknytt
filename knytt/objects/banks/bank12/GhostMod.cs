using Godot;
using System;

public class GhostMod : Node2D
{
    [Export] bool flickering = true;

    public float flickerMin = .1f;
    public float flickerMax = .8f;
    public float flip_time = .065f;
    float flip_counter = 0f;
    float flip_target = 0f;
    public float change_fraction = 8f;

    protected GDKnyttBaseObject parent;
    protected Juni globalJuni;

    public override void _Ready()
    {
        parent = GetParent<GDKnyttBaseObject>();
        globalJuni = parent.Juni;

        if (!globalJuni.Powers.getPower(YKnyttLib.JuniValues.PowerNames.Eye))
        {
            parent.QueueFree();
            return;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (flickering) { handleFlicker(delta); }
    }

    private void handleFlicker(float delta)
    {
        flip_counter += delta;
        while(flip_counter > flip_time) 
        { 
            flip_counter -= flip_time;
            this.flip_target = GDKnyttDataStore.random.Next(2) == 0 ? flickerMin : flickerMax;
        }

        var m = parent.Modulate;
        m.a += (flip_target - m.a) * change_fraction * delta;
        parent.Modulate = m;
    }
}
