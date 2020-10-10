using Godot;

public class CloudParticles : Node2D
{
    CPUParticles2D particles;

    public override void _Ready()
    {
        particles = GetNode<CPUParticles2D>("CPUParticles2D");
        particles.Emitting = true;
    }

    public override void _Process(float delta)
    {
        if (!particles.Emitting) { QueueFree(); }
    }
}
