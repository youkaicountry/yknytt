using Godot;
using System.Collections.Generic;

public partial class SavePoint : GDKnyttBaseObject
{
    HashSet<Juni> junis;
    AnimationPlayer animation;

    public override void _Ready()
    {
        junis = new HashSet<Juni>();
        animation = GetNode<AnimationPlayer>("Sprite2D/AnimationPlayer");
        animation.Play("Idle");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (junis.Count == 0 || animation.CurrentAnimation.Equals("Save")) { return; }
        foreach (var juni in junis)
        {
            if (juni.juniInput.DownPressed) { save(juni); }
        }
    }

    private async void save(Juni juni)
    {
        GDArea.GDWorld.Game.saveGame(juni.GDArea.Area.Position, Coords, true);
        GetNode<AudioStreamPlayer2D>("SavePlayer2D").Play();
        animation.Play("Save");
        await ToSignal(animation, "animation_finished");
        animation.Play("Idle");
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni)) { return; }
        junis.Add((Juni)body);
    }

    public void _on_Area2D_body_exited(Node body)
    {
        if (!(body is Juni)) { return; }
        junis.Remove((Juni)body);
    }
}
