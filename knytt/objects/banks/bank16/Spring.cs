using Godot;
using System.Collections.Generic;

public partial class Spring : GDKnyttBaseObject
{
    HashSet<Juni> junis;

    public override void _Ready()
    {
        junis = new HashSet<Juni>();
        var sprite = GetNode<Sprite2D>("Sprite2D");
        var m = sprite.Modulate;
        m.A = 0f;
        sprite.Modulate = m;
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (Juni juni in junis)
        {
            // Enforce one-way
            var jgp = juni.GlobalPosition;
            var gp = GlobalPosition;
            if (juni.velocity.Y <= 0 || jgp.Y > gp.Y + 10f) { return; }

            // Restore Juni's position to the top of the spring for stability
            var jbottom = juni.Bottom;
            jgp.Y -= jbottom.Y - gp.Y;
            juni.GlobalPosition = jgp;

            // Spring
            juni.executeJump(-340f, sound: false, reset_jumps: true);
            GetNode<AudioStreamPlayer2D>("BouncePlayer2D").Play();

            var anim = GetNode<AnimationPlayer>("AnimationPlayer");
            anim.Stop();
            anim.Play("Spring");
        }
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni)) { return; }
        junis.Add(body as Juni);
    }

    public void _on_Area2D_body_exited(Node body)
    {
        if (!(body is Juni)) { return; }
        junis.Remove(body as Juni);
    }
}
