using Godot;

public class SceneCPUParticleInstance : Node2D
{
    public Vector2 Velocity;
    public float Mass;
    public Vector2 Force;
    public Vector2 Gravity;
    public float Drag;

    public string Params;

    protected Vector2 _acceleration;

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        // Simple Euler Integration
        _acceleration = Force / Mass;
        _acceleration += Gravity;
        Velocity += (_acceleration - (Drag * Velocity)) * delta;
        Translate(Velocity * delta);
    }
}
