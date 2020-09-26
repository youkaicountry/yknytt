using Godot;

public class CosineMod : Node2D
{
    Node2D parent;

    [Export] public float magnitude = 1f;
    [Export] public float frequency = 1f;

    [Export] public bool autoStart = false;

    float time = 0f;
    bool running = false;
    float original_y = 0f;

    public override void _Ready()
    {
        parent = GetParent<Node2D>();
        if (autoStart) { start(); }
    }

    public override void _PhysicsProcess(float delta)
    {
        time += delta;
        if (!running) { return; }
        var p = parent.Position;
        parent.Position = new Vector2(p.x, original_y + Mathf.Sin(time*frequency*delta) * magnitude);
    }

    public void start()
    {
        running = true;
        original_y = parent.Position.y;
    }

    public void stop()
    {
        running = false;
        var p = parent.Position;
        p.y = original_y;
        parent.Position = p;
    }
}
