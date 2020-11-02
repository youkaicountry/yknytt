using System.Collections.Generic;
using Godot;
using YUtil.Random;

public class Leaf : GDKnyttBaseObject
{
    SceneCPUParticles particles;

    struct LeafData
    {
        public float min_time, max_time;

        public LeafData(float min_time, float max_time)
        {
            this.min_time = min_time;
            this.max_time = max_time;
        }
    }

    static Dictionary<int, LeafData> ID2Data;

    static Leaf()
    {
        ID2Data = new Dictionary<int, LeafData>();
        ID2Data[1] = new LeafData(1f, 3f);
        ID2Data[6] = new LeafData(.5f, 5f);
        ID2Data[10] = new LeafData(1f, 8f);
        ID2Data[12] = new LeafData(1f, 3f);
    }

    LeafData CurrentData;

    public override void _Ready()
    {
        base._Ready();
        particles = GetNode<SceneCPUParticles>($"SceneCPUParticles{ObjectID.y}");
        particles.ParticleParams = $"{ObjectID.y}";
        CurrentData = ID2Data[ObjectID.y];
        startSpawnDelay(CurrentData.min_time / 2f, CurrentData.max_time / 2f);
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
        startSpawnDelay(CurrentData.min_time, CurrentData.max_time);
    }
}
