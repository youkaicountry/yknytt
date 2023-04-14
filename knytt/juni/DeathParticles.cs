using Godot;

public partial class DeathParticles : Node2D
{
    public void Play()
    {
        GetNode<CpuParticles2D>("DeathParticles1").Restart();
        GetNode<CpuParticles2D>("DeathParticles2").Restart();
        GetNode<CpuParticles2D>("DeathParticles3").Restart();
        GetNode<CpuParticles2D>("DeathParticles4").Restart();
    }
}
