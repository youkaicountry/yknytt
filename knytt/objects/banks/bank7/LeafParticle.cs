using Godot;
using System.Collections.Generic;

public partial class LeafParticle : Node2D
{
    static Dictionary<string, Color> colors;

    static LeafParticle()
    {
        colors = new Dictionary<string, Color>();
        colors["1"] = new Color(.05f, .05f, .05f);
        colors["6"] = new Color(.45f, .34f, .64f);
        colors["10"] = new Color(.2f, .34f, .16f);
        colors["12"] = new Color(.95f, .95f, .95f);
    }

    public override void _Ready()
    {
        string p = GetParent<SceneCPUParticleInstance>().Params;
        Modulate = colors[p];
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Animation = p;
    }

    public void _on_Area2D_body_entered(Node body)
    {
        GetParent().QueueFree();
    }
}
