using Godot;
using YUtil.Random;

public class Leaf : GDKnyttBaseObject
{
    SceneCPUParticles particles;

    const float MIN_TIME = 2f;
    const float MAX_TIME = 12f;

    public override void _Ready()
    {
        base._Ready();
        particles = GetNode<SceneCPUParticles>("SceneCPUParticles");
        particles.ParticleParams = $"{ObjectID.y}";
        startSpawnDelay(MIN_TIME / 2f, MAX_TIME / 2f);
    }

    private void startSpawnDelay(float min, float max)
    {
        var timer = GetNode<Timer>("SpawnTimer");
        timer.WaitTime = GDKnyttDataStore.random.NextFloat(min, max);
        timer.Start();
    }

    public void _on_SpawnTimer_timeout()
    {
        particles.spawnParticles();
        startSpawnDelay(MIN_TIME, MAX_TIME);
    }
}
