using Godot;
using YUtil.Random;

public partial class Dust : GDKnyttBaseObject
{
    SceneCPUParticles particles;

    private const float min_time = .2f, max_time = .5f;

    public override void _Ready()
    {
        particles = GetNode<SceneCPUParticles>("DustEmitter");

        if (ObjectID.y == 14) { startSpawnDelay(min_time / 2f, max_time / 2f); }
        else { particles.spawnParticles(); }
    }

    private void startSpawnDelay(float min, float max)
    {
        var timer = GetNode<Timer>("SpawnTimer");
        timer.WaitTime = random.NextFloat(min, max);
        timer.Start();
    }

    public void _on_SpawnTimer_timeout()
    {
        particles.spawnParticles();
        startSpawnDelay(min_time, max_time);
    }
}
