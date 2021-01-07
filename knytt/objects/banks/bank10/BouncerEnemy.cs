using Godot;
using YUtil.Random;
using System.Collections.Generic;

public class BouncerEnemy : GDKnyttBaseObject
{
    private const float MIN_JUMP = 6.5f*24f, MAX_JUMP = 9.5f*24f;

    struct BouncerData
    {
        public bool jump_trigger;
        public float gravity;
        public float jump_force;
        public bool deadly;

        public BouncerData(bool jump_trigger, float gravity, float jump_force, bool deadly)
        {
            this.jump_trigger = jump_trigger;
            this.gravity = gravity;
            this.jump_force = jump_force;
            this.deadly = deadly;
        }
    }

    static Dictionary<int, BouncerData> bouncers;

    private string Anim { get { return $"Bouncer{ObjectID.y}"; } }

    BouncerData bouncer_data;
    float vel;
    bool in_air = false;
    float start_y;

    static BouncerEnemy()
    {
        bouncers = new Dictionary<int, BouncerData>();
        bouncers.Add(5, new BouncerData(true, 600f, 100f, true));
    }

    public override void _Ready()
    {
        base._Ready();
        bouncer_data = bouncers[ObjectID.y];
        if (bouncer_data.deadly) { OrganicEnemy = true; }
        
        if (bouncer_data.jump_trigger)
        {
            // Connect to each Juni
            Juni.Connect("Jumped", this, "juniJumped");
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (!in_air) { return; }

        vel += bouncer_data.gravity * delta;
        Translate(new Vector2(0f, vel * delta));
    }

    public void launch()
    {
        start_y = Position.y;
        vel = -bouncer_data.jump_force;
        in_air = true;
        GetNode<AnimatedSprite>("AnimatedSprite").Play(Anim);
    }

    public void juniJumped(Juni juni)
    {
        if (in_air) { return; }

        // Calculate and test jump chance
        var dist = Juni.distance(Center, false);
        if (dist > 24f*7)
        {
            var i = 1f - Mathf.Min((dist - MIN_JUMP) / (MAX_JUMP - MIN_JUMP), 1f);
            if (random.NextFloat() > i) { return; }
        }

        launch();
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (bouncer_data.deadly && body is Juni juni)
        {
            juniDie(juni);
            return;
        }

        if (!in_air) { return;}
        if (!GDArea.isIn(GlobalPosition)) { return; } // ignore all collisions if out of the area
        if (vel < 0f) { vel = -vel; return; }

        in_air = false;
        
        var p = Position;
        p.y = start_y;
        Position = p;

        GetNode<AnimatedSprite>("AnimatedSprite").Play(Anim, true);
    }
}
