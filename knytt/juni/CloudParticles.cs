using Godot;

public partial class CloudParticles : Node2D
{
    CpuParticles2D particles;

    public override void _Ready()
    {
        particles = GetNode<CpuParticles2D>("CPUParticles2D");
        particles.Emitting = true;
    }

    public override void _Process(double delta)
    {
        if (!particles.Emitting) { QueueFree(); }
    }
}
