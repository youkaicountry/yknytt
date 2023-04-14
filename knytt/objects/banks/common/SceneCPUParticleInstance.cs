using Godot;

public partial class SceneCPUParticleInstance : Node2D
{
    public Vector2 Velocity;
    public float Mass;
    public Vector2 Force;
    public Vector2 Gravity;
    public float Drag;

    public FastNoiseLite _noise;

    bool stopped = false;

    bool _brownian;
    public bool BrownianMotion
    {
        get { return _brownian; }
        set
        {
            if (value)
            {
                if (_noise == null)
                {
                    _noise = new FastNoiseLite();
                    _noise.Seed = GDKnyttDataStore.random.Next();
                }
            }
            _brownian = value;
        }
    }

    public Vector2 BrownianForce;
    public Vector2 BrownianSpeed;
    public float BrownianExponent;
    protected Vector2 _brownian_t;

    public float Lifetime;

    public string Params;

    protected Vector2 _acceleration;
    protected float _time;

    public override void _Ready()
    {
        _brownian_t = new Vector2(0f, 999999f);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (stopped) { return; }

        base._PhysicsProcess(delta);
        _time += (float)delta;
        if (_time >= Lifetime) { QueueFree(); }

        if (_brownian) { calcBrownianForces((float)delta); }

        // Simple Euler Integration
        _acceleration = Force / Mass;
        _acceleration += Gravity;
        Velocity += (_acceleration - (Drag * Velocity)) * (float)delta;
        Translate(Velocity * (float)delta);

        Force = Vector2.Zero;
    }

    private void calcBrownianForces(float delta)
    {
        _brownian_t += BrownianSpeed * delta;


        var bm = new Vector2(_noise.GetNoise1D(_brownian_t.X), _noise.GetNoise1D(_brownian_t.Y)) * BrownianForce;
        bm.X = Mathf.Pow(Mathf.Abs(bm.X), BrownianExponent) * Mathf.Sign(bm.X);
        bm.Y = Mathf.Pow(Mathf.Abs(bm.Y), BrownianExponent) * Mathf.Sign(bm.Y);
        Force += bm;
    }

    public void stop()
    {
        stopped = true;
    }
}
