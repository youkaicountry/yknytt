using Godot;

public class DeathParticles : Node2D
{
    public void Play()
    {
        Visible = true;
        GetNode<CPUParticles2D>("DeathParticles1").Restart();
        GetNode<CPUParticles2D>("DeathParticles2").Restart();
        GetNode<CPUParticles2D>("DeathParticles3").Restart();
        GetNode<CPUParticles2D>("DeathParticles4").Restart();
    }
}
