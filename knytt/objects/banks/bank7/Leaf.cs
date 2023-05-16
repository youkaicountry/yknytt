using Godot;

public class Leaf : GDKnyttBaseObject
{
    [Export] int yGenegrator;
    [Export] int ySingle;

    public override void _Ready()
    {
        base._Ready();
        if (ObjectID.y == yGenegrator)
        {
            GetNode<TimerExt>("SpawnTimer").RunTimer();
        }
        if (ObjectID.y == ySingle)
        {
            GetNode<SceneCPUParticles>("SceneCPUParticles").spawnParticles();
        }
    }

    private void _on_SpawnTimer_timeout_ext()
    {
        GetNode<SceneCPUParticles>("SceneCPUParticles").spawnParticles();
    }
}
