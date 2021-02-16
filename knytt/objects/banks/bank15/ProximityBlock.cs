using Godot;
using System.Collections.Generic;

public class ProximityBlock : GDKnyttBaseObject
{
    private const float RADIUS = 132f;

    HashSet<Juni> junis;
    Vector2 _center;
    bool real;

    private float Proximity
    {
        set
        {
            var m = Modulate;
            m.a = real ? value : (1f - value);
            Modulate = m;
        }
    }

    public override void _Ready()
    {
        junis = new HashSet<Juni>();
        _center = Center;
        real = ObjectID.y == 6;
        if (!real) { GetNode<Node>("StaticBody2D").QueueFree(); }
        Proximity = 0f;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (junis.Count == 0) { return; }

        float closest = RADIUS;
        foreach (Juni juni in junis)
        {
            var distance = juni.distance(_center, false);
            if (distance < closest) { closest = distance; }
        }

        Proximity = Mathf.InverseLerp(RADIUS, 0f, closest);
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (body is Juni) { junis.Add((Juni)body); }
    }

    public void _on_Area2D_body_exited(Node body)
    {
        if (body is Juni) { junis.Remove((Juni)body); }
        if (junis.Count == 0) { Proximity = 0f; }
    }
}
