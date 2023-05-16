using Godot;
using System.Collections.Generic;

public class LeafParticle : Node2D
{
    static Dictionary<string, Color> colors = new Dictionary<string, Color> {
        ["6"] = new Color(.45f, .34f, .64f),
        ["10"] = new Color(.2f, .34f, .16f),
        ["12"] = new Color(.95f, .95f, .95f)
    };

    public override void _Ready()
    {
        string p = GetParent<SceneCPUParticleInstance>().Params;
        if (colors.ContainsKey(p)) { Modulate = colors[p]; }
        GetNode<AnimatedSprite>("AnimatedSprite").Animation = (p == "6" || p == "10" || p == "12") ? "default" : p;
    }

    public void _on_Area2D_body_entered(Node body) // TODO: collide with water
    {
        GetParent().QueueFree();
    }
}
