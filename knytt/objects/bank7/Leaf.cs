using Godot;

public partial class Leaf : GDKnyttBaseObject
{
    [Export] int yGenegrator;
    [Export] int ySingle;

    public override void _Ready()
    {
        base._Ready();
        if (ObjectID.Y == yGenegrator)
        {
            GetNode<TimerExt>("SpawnTimer").RunTimer();
        }
        if (ObjectID.Y == ySingle)
        {
            GetNode<SceneCPUParticles>("SceneCPUParticles").spawnParticles();
        }
    }

    private void _on_SpawnTimer_timeout_ext()
    {
        GetNode<SceneCPUParticles>("SceneCPUParticles").spawnParticles();
    }
}
