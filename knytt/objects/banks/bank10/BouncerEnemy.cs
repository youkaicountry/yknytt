using Godot;
using YKnyttLib;

public partial class BouncerEnemy : GDKnyttBaseObject
{
    [Export] float gravity;
    [Export] float jump_force;
    [Export] float extra_gravity;
    [Export] float extra_jump_force;

    float vel;
    bool in_air = false;
    float start_y;

    private AnimatedSprite2D sprite;

    public override void _Ready()
    {
        base._Ready();
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        Juni.Jumped += juniJumped;
    }

    private void _on_tree_exiting()
    {
        Juni.Jumped -= juniJumped;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (!in_air) { return; }

        vel += (Juni.Powers.getPower(JuniValues.PowerNames.DoubleJump) ? extra_gravity : gravity) * (float)delta;
        Translate(new Vector2(0f, vel * (float)delta));
    }

    public void launch()
    {
        start_y = Position.Y;
        vel = -(Juni.Powers.getPower(JuniValues.PowerNames.DoubleJump) ? extra_jump_force : jump_force);
        in_air = true;
        GetNodeOrNull<AudioStreamPlayer2D>("JumpPlayer")?.Play();
        sprite.Play("jump");
    }

    public void juniJumped(Juni juni)
    {
        if (in_air) { return; }

        // Calculate and test jump chance
        if (!in_air && juni.Hologram == null && Mathf.Abs(juni.ApparentPosition.X - Center.X) < 150 + random.Next(80))
        {
            launch();
        }
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (body is Juni juni)
        {
            juniDie(juni);
            return;
        }

        if (!in_air) { return; }
        if (!GDArea.isIn(GlobalPosition)) { return; } // ignore all collisions if out of the area
        if (vel < 0f) { vel = -vel; return; }

        in_air = false;

        Position = new Vector2(Position.X, start_y);

        GetNodeOrNull<AudioStreamPlayer2D>("BouncePlayer")?.Play();
        sprite.PlayBackwards("stop");
    }
}
