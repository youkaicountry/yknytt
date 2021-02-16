using Godot;

public class JuniMotionParticles : Node2D
{
    int _climb_particle;
    int _climb_areas;
    public int ClimbParticle
    {
        get { return _climb_areas > 0 ? _climb_particle : -1; }
        private set { _climb_particle = value; }
    }

    int _ground_particle;
    int _ground_areas;
    public int GroundParticle
    {
        get { return _ground_areas > 0 ? _ground_particle : -1; }
        private set { _ground_particle = value; }
    }

    public enum JuniMotion
    {
        NONE,
        WALK,
        RUN,
        CLIMB
    }

    public Vector2 ClimbPosition
    {
        get { return GetNode<Node2D>(Juni.FacingRight ? "SpawnLocations/LeftClimb" : "SpawnLocations/LeftClimb").GlobalPosition; }
    }

    public Vector2 GroundPosition { get { return GetNode<Node2D>("SpawnLocations/Ground").GlobalPosition; } }

    JuniMotion _current_motion = JuniMotion.NONE;
    public JuniMotion CurrentMotion
    {
        get { return _current_motion; }
        set { setMotion(value); }
    }

    public Juni Juni { get; private set; }

    public static readonly PackedScene[] ParticleScenes = new PackedScene[]
    {
        ResourceLoader.Load<PackedScene>("res://knytt/juni/CloudParticles.tscn")
    };

    public override void _Ready()
    {
        Juni = GetParent<Juni>();
    }

    private void setMotion(JuniMotion motion)
    {
        if (_current_motion == motion) { return; }
        _current_motion = motion;
        switch (motion)
        {
            case JuniMotion.NONE:
                foreach (var child in GetNode("Timers").GetChildren())
                {
                    ((Timer)child).Stop();
                }
                break;

            case JuniMotion.WALK:
                GetNode<Timer>("Timers/WalkTimer").Start();
                break;

            case JuniMotion.RUN:
                GetNode<Timer>("Timers/RunTimer").Start();
                break;

            case JuniMotion.CLIMB:
                GetNode<Timer>("Timers/ClimbTimer").Start();
                break;
        }
    }

    public void _on_Timer_timeout()
    {
        switch (CurrentMotion)
        {
            case JuniMotion.NONE: return;

            case JuniMotion.WALK:
            case JuniMotion.RUN:
                emitParticles(GroundParticle, GroundPosition);
                break;

            case JuniMotion.CLIMB:
                emitParticles(ClimbParticle, ClimbPosition);
                break;
        }
    }

    public void emitParticles(int p, Vector2 global_pos)
    {
        if (p < 0) { return; }
        var p_node = ParticleScenes[p].Instance() as Node2D;
        p_node.GlobalPosition = global_pos;
        GetNode("Particles").AddChild(p_node);
    }

    public void _on_ClimbParticles_area_shape_entered(int area_id, Area2D area, int area_shape, int self_shape)
    {
        ClimbParticle = ((IParticleFetch)area).ParticleType;
        _climb_areas++;
    }

    public void _on_ClimbParticles_area_shape_exited(int area_id, Area2D area, int area_shape, int self_shape)
    {
        _climb_areas--;
    }

    public void _on_GroundParticles_area_shape_entered(int area_id, Area2D area, int area_shape, int self_shape)
    {
        GroundParticle = ((IParticleFetch)area).ParticleType;
        _ground_areas++;
    }

    public void _on_GroundParticles_area_shape_exited(int area_id, Area2D area, int area_shape, int self_shape)
    {
        _ground_areas--;
    }
}
