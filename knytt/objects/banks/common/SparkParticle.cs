using Godot;

public class SparkParticle : Sprite
{
    bool dying = false;

    public void _on_Area2D_body_entered(Node body)
    {
        if (dying) { return; }
        if (body is Juni) { ((Juni)body).die(); return;}
        die();
    }

    public async void die()
    {
        if (dying) { return; }
        dying = true;
        GetParent<SceneCPUParticleInstance>().stop();
        var anim = GetNode<AnimationPlayer>("AnimationPlayer");
        anim.Play("Die");
        await ToSignal(anim, "animation_finished");
        GetParent().QueueFree();
    }
}
